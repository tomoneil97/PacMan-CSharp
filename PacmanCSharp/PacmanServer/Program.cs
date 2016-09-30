﻿using Pacman.GameLogic;
using Pacman.GameLogic.RemoteControl;
using PacmanAI;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace StarCraftServer
{
    class Program
    {
        public static string HostName = "";

        public static void LoadParameters(string[] args)
        {
            if (args.Length > 0)
            {
                HostName = args[0];
            }
            else
            {
                Console.Write("Host Name: ");

                HostName = Console.ReadLine();
            }
        }

        private static int gamesPlayed = 0;
        private static int totalScore = 0;
        private static GameState gs;
        private static int gamesToPlay = 100;
        private static long longestGame = 0;
        
        private static int highestScore = 0;
        private static int lowestScore = int.MaxValue;

        private static int maxPillsEaten = 0;
        private static int minPillsEaten = int.MaxValue;
        private static int pillsEatenTotal = 0;

        private static int maxGhostsEaten = 0;
        private static int minGhostsEaten = int.MaxValue;
        private static int totalGhostsEaten = 0;

        private static long lastMs = 0;
        private static long ms = 0;
        
        public static string Results = "";
        

        public static void Reset()
        {
            gamesPlayed = 0;
            totalScore = 0;
            longestGame = 0;
            highestScore = 0;
            lowestScore = int.MaxValue;
            maxPillsEaten = 0;
            minPillsEaten = int.MaxValue;
            pillsEatenTotal = 0;
            maxGhostsEaten = 0;
            minGhostsEaten = int.MaxValue;
            totalGhostsEaten = 0;

            Results = "";
        }

        private static void GameOverParallelHandler(object sender, EventArgs args)
        {
            GameState GS = sender as GameState;

            GS.PausePlay();

            highestScore = Math.Max(highestScore, GS.Pacman.Score);
            lowestScore = Math.Min(lowestScore, GS.Pacman.Score);

            totalScore += GS.Pacman.Score;
            gamesPlayed++;
            
            Results += GS.m_GhostsEaten + "," + GS.m_PillsEaten + "," + GS.Pacman.Score + "\n";
        }

        public static void RunGamesParallel(RPCData CustomData)
        {
            Reset();
            try
            {
                gamesToPlay = CustomData.GamesToPlay;
                BasePacman controller = null;

                if (CustomData.AIToUse.Equals("LucPacScripted"))
                {
                    controller = new LucPacScripted();
                    LucPacScripted.REMAIN_QUIET = true;
                }
                if (CustomData.AIToUse.Equals("MMPac"))
                {
                    controller = new MMPac.MMPac(); //TODO: take default best NN Data
                }
                if (CustomData.AIToUse.Equals("MMLocPac"))
                {
                    controller = new MMPac.MMLocPac(); //TODO: take default best NN Data
                }
                if (CustomData.AIToUse.Equals("LucPac"))
                {
                    controller = new LucPac();
                }

                List<GameState> PGames = new List<GameState>();

                for(int i=0;i<gamesToPlay;i++)
                {
                    var GS = new GameState(CustomData.MapData.EvolvedValues, 0);
                    GS.StartPlay();
                    GS.Controller = controller;
                    GS.GameOver += new EventHandler(GameOverParallelHandler);
                    PGames.Add(GS);
                }

                bool Over = false;

                while(!Over) 
                {
                    foreach (var GS in PGames)
                    {
                        Direction direction = controller.Think(GS);
                        GS.Pacman.SetDirection(direction);
                        
                        GS.Update();
                    }

                    var AvgScore = PGames.Where(c => c.Started).Sum(c => c.Pacman.Score);
                    AvgScore += totalScore;
                    AvgScore = AvgScore / gamesToPlay;

                    Over = (AvgScore > CustomData.MaxScore);

                    Over = Over || gamesToPlay == gamesPlayed;

                    Console.Clear();
                    Console.WriteLine("Simulating ... ");
                    Console.WriteLine(" - Current avg: " + AvgScore);
                }

                if(gamesPlayed < gamesToPlay)
                {
                    //Early exit done
                    foreach(var GS in PGames.Where(c => c.Started))
                    {
                        GameOverParallelHandler(GS, null);
                    }
                }

                controller.SimulationFinished();

                Console.Clear();
                Console.WriteLine("Games played: " + gamesPlayed);
                Console.WriteLine("Avg. score: " + (totalScore / gamesPlayed));
                Console.WriteLine("Highest score: " + highestScore + " points");
                Console.WriteLine("Lowest score: " + lowestScore + " points");
            }
            catch(Exception e) {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        public static void RunGameLinear(RPCData CustomData)
        {
            Reset();
            try
            {
                // Set the new count of games that we want to simulate.
                gamesToPlay = CustomData.GamesToPlay;
                

                // Get some strange invocation error here.
                // tryLoadController(_agentName);
                
                BasePacman controller = null;

                if(CustomData.MapData.EvolvedValues.Count < 25 && CustomData.MapData.EvolvedValues.Count > 0)
                {
                    gs = new GameState(CustomData.MapData.EvolvedValues, CustomData.RandomSeed);
                } else
                {
                    gs = new GameState(CustomData.RandomSeed);
                }

                //BasePacman controller = new TestPac();
                if(CustomData.AIToUse.Equals("LucPacScripted"))
                {
                    controller = new LucPacScripted();
                    LucPacScripted.REMAIN_QUIET = true;
                }
                if (CustomData.AIToUse.Equals("MMPac"))
                {
                    //use CustomData.NeuralNetwork in constructor
                    controller = new MMPac.MMPac(CustomData.MapData.EvolvedValues);
                }
                if (CustomData.AIToUse.Equals("MMLocPac"))
                {
                    //use CustomData.NeuralNetwork in constructor
                    if (CustomData.MapData.EvolvedValues.Count < 25)
                        controller = new MMPac.MMLocPac("NeuralNetworkLocPac.nn");
                    else
                        controller = new MMPac.MMLocPac(CustomData.MapData.EvolvedValues);
                }
                if(CustomData.AIToUse.Equals("LucPac"))
                {
                    controller = new LucPac();
                    LucPac.REMAIN_QUIET = true;
                }

                
                gs.GameOver += new EventHandler(GameOverHandler);
                gs.StartPlay();

                gs.Controller = controller;

                Stopwatch watch = new Stopwatch();
                int percentage = -1;
                int lastUpdate = 0;
                watch.Start();
                while (gamesPlayed < gamesToPlay)
                {
                    int newPercentage = (int)Math.Floor(((float)gamesPlayed / gamesToPlay) * 100);
                    if (newPercentage != percentage || gamesPlayed - lastUpdate >= 100)
                    {
                        lastUpdate = gamesPlayed;
                        percentage = newPercentage;
                        Console.Clear();
                        /*Console.Write("Current parameter set: ");
                        foreach(var X in CustomData.MapData.EvolvedValues)
                        {
                            Console.Write(X + ", ");
                        }*/
                        Console.WriteLine();
                        Console.WriteLine("Simulating ... " + percentage + "% (" + gamesPlayed + " : " + gamesToPlay + ")");
                        //Console.WriteLine(" - Elapsed: " + formatSeconds((watch.ElapsedMilliseconds / 1000.0) + "") + "s, Estimated total: " + formatSeconds(((watch.ElapsedMilliseconds / 1000.0) / percentage * 100) + "") + "s");
                        Console.WriteLine(" - Current best: " + highestScore);
                        Console.WriteLine(" - Current worst: " + lowestScore);
                        if (gamesPlayed > 0)
                        {
                            Console.WriteLine(" - Current avg.: " + (totalScore / gamesPlayed));
                        }
                        /*for (int i = scores.Count - 1; i >= 0 && i > scores.Count - 100; i--)
                        {
                            Console.Write(scores[i] + ",");
                        }*/
                    }
                    // update gamestate
                    Direction direction = controller.Think(gs);
                    gs.Pacman.SetDirection(direction);

                    // update game
                    gs.Update();
                    ms += GameState.MSPF;
                }
                watch.Stop();

                // shut down controller
                controller.SimulationFinished();

                // output results
                Console.Clear();
                long seconds = ms / 1000;
                Console.WriteLine("Games played: " + gamesPlayed);
                Console.WriteLine("Avg. score: " + (totalScore / gamesPlayed));
                Console.WriteLine("Highest score: " + highestScore + " points");
                Console.WriteLine("Lowest score: " + lowestScore + " points");
                Console.WriteLine("Max Pills Eaten: " + maxPillsEaten);
                Console.WriteLine("Min Pills Eaten: " + minPillsEaten);
                Console.WriteLine("Average Pills Eaten: " + pillsEatenTotal / gamesPlayed);
                Console.WriteLine("Max Ghosts Eaten: " + maxGhostsEaten);
                Console.WriteLine("Min Ghosts Eaten: " + minGhostsEaten);
                Console.WriteLine("Average Ghosts Eaten: " + totalGhostsEaten / gamesPlayed);
                Console.WriteLine("Longest game: " + ((float)longestGame / 1000.0f) + " seconds");
                Console.WriteLine("Total simulated time: " + (seconds / 60 / 60 / 24) + "d " + ((seconds / 60 / 60) % 24) + "h " + ((seconds / 60) % 60) + "m " + (seconds % 60) + "s");
                Console.WriteLine("Avg. simulated time pr. game: " + ((float)ms / 1000.0f / gamesPlayed) + " seconds");
                Console.WriteLine("Simulation took: " + (watch.ElapsedMilliseconds / 1000.0f) + " seconds");
                //Console.WriteLine("Speed: " + (ms / watch.ElapsedMilliseconds) + " (" + ((ms / watch.ElapsedMilliseconds) / 60) + "m " + ((ms / watch.ElapsedMilliseconds) % 60) + " s) simulated seconds pr. second");
                Console.WriteLine("For a total of: " + gamesPlayed / (watch.ElapsedMilliseconds / 1000.0f) + " games pr. second");

            }
            catch (Exception e)
            {
                // Log error.
                Console.WriteLine("Error happened. " + e.Message);
            }
            //return Results;
        }

        static void Main(string[] args)
        {
            LoadParameters(args);

            var factory = new ConnectionFactory()
            {
                HostName = HostName
            };

            do
            {
                //On error, try to reconnect to server. Wait 5 seconds between reconnect attempts
                Console.WriteLine("Connecting to host " + HostName);
                try
                {


                    using (var connection = factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: "rpc_queue",
                                             durable: false,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: null);
                        channel.BasicQos(0, 1, false);
                        var consumer = new QueueingBasicConsumer(channel);
                        channel.BasicConsume(queue: "rpc_queue",
                                             noAck: false,
                                             consumer: consumer);
                        Console.WriteLine(" [x] Awaiting RPC requests");

                        while (true)
                        {
                            string response = null;
                            var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();

                            var body = ea.Body;
                            var props = ea.BasicProperties;
                            var replyProps = channel.CreateBasicProperties();
                            replyProps.CorrelationId = props.CorrelationId;

                            try
                            {
                                var message = Encoding.UTF8.GetString(body);

                                //int n = int.Parse(message);
                                Console.WriteLine(" [.] RunGame()");

                                var serializer = new XmlSerializer(typeof(RPCData));
                                RPCData customData;

                                using (TextReader reader = new StringReader(message))
                                {
                                    customData = (RPCData)serializer.Deserialize(reader);
                                }

                                if(!customData.Parallel)
                                {
                                    RunGameLinear(customData);
                                } else
                                {
                                    RunGamesParallel(customData);
                                }
                                
                                response = Program.Results;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(" [.] " + e.Message);
                                response = "";
                            }
                            finally
                            {
                                var responseBytes = Encoding.UTF8.GetBytes(response);
                                channel.BasicPublish(exchange: "",
                                                     routingKey: props.ReplyTo,
                                                     basicProperties: replyProps,
                                                     body: responseBytes);
                                channel.BasicAck(deliveryTag: ea.DeliveryTag,
                                                 multiple: false);
                            }

                            Console.WriteLine(" [x] Awaiting RPC requests");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: " + e.Message);
                }

                Thread.Sleep(5000);
            } while (true);
        }

        private static void GameOverHandler(object sender, EventArgs args)
        {
            longestGame = Math.Max(longestGame, ms - lastMs);

            highestScore = Math.Max(highestScore, gs.Pacman.Score);
            lowestScore = Math.Min(lowestScore, gs.Pacman.Score);

            totalScore += gs.Pacman.Score;

            maxPillsEaten = Math.Max(gs.m_PillsEaten, maxPillsEaten);
            minPillsEaten = Math.Min(gs.m_PillsEaten, minPillsEaten);

            maxGhostsEaten = Math.Max(gs.m_GhostsEaten, maxGhostsEaten);
            minGhostsEaten = Math.Min(gs.m_GhostsEaten, minGhostsEaten);
            
            pillsEatenTotal += gs.m_PillsEaten;

            totalGhostsEaten += gs.m_GhostsEaten;

            //scores.Add(gs.Pacman.Score);
            //totalScore += gs.Pacman.Score;
            gamesPlayed++;
            lastMs = ms;

            Results += gs.m_GhostsEaten + "," + gs.m_PillsEaten + "," + gs.Pacman.Score + "\n";
        }
    }
}