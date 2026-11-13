using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Classes;
using Newtonsoft.Json;

namespace Snake
{
    internal class Program
    {
        public static List<Leaders> Leaders = new List<Leaders>();
        public static List<ViewModelUserSettings> remoteIpAddress = new List<ViewModelUserSettings>();
        public static List<ViewModelGames> viewModelGames = new List<ViewModelGames>();
        public static int localport = 5001;
        public static int MaxSpeed = 15;
        static void Main(string[] args)
        {
            try
            {
                Thread tRec = new Thread(new ThreadStart(Reciever));
                tRec.Start();

                Thread tTime = new Thread(Timer);
                tTime.Start();
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n " + ex.Message);
            }
        }

        private static void Send()
        {
            foreach (ViewModelUserSettings User in remoteIpAddress)
            {
                UdpClient sender = new UdpClient();
                IPEndPoint endPoint = new IPEndPoint(
                    IPAddress.Parse(User.IPAddress),
                    int.Parse(User.Port));
                try
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(viewModelGames.Find(x => x.IdSnake == User.IdSnake)));

                    sender.Send(bytes, bytes.Length, endPoint);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Отпраавил данные пользователю: {User.IPAddress}:{User.Port}");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
                }
                finally
                {
                    sender.Close();
                }
            }

        }

        public static void Reciever()
        {
            UdpClient recievingUdpClient = new UdpClient(localport);

            IPEndPoint RemoteIpEndPoint = null;

            try
            {
                Console.WriteLine("Команды сервера: ");
                while (true)
                {
                    byte[] recieveBytes = recievingUdpClient.Receive(
                        ref RemoteIpEndPoint);

                    string returnData = Encoding.UTF8.GetString(recieveBytes);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Получил команду: " + returnData.ToString());

                    if (returnData.ToString().Contains("/start"))
                    {
                        string[] dataMessage = returnData.ToString().Split('|');
                        ViewModelUserSettings viewModelUserSettings = JsonConvert.DeserializeObject<ViewModelUserSettings>(dataMessage[1]);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"подключился пользователь: {viewModelUserSettings.IPAddress}: {viewModelUserSettings.Port}");
                        remoteIpAddress.Add(viewModelUserSettings);
                        viewModelUserSettings.IdSnake = AddSnake();
                        viewModelGames[viewModelUserSettings.IdSnake].IdSnake = viewModelUserSettings.IdSnake;
                    }
                    else
                    {
                        string[] dataMessage = returnData.ToString().Split('|');

                        ViewModelUserSettings viewModelUserSettings = JsonConvert.DeserializeObject<ViewModelUserSettings>(dataMessage[1]);

                        int IdPlayer = -1;
                        IdPlayer = remoteIpAddress.FindIndex(x => x.IPAddress == viewModelUserSettings.IPAddress
                        && x.Port == viewModelUserSettings.Port);

                        if (IdPlayer != -1)
                        {
                            if (dataMessage[0]=="Up" &&
                                viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Down)
                            {
                                viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Up;
                            }
                            else if (dataMessage[0] == "Down" &&
                                viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Up)
                            {
                                viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Down;
                            }
                            else if(dataMessage[0] == "Left" &&
                                viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Right)
                            {
                                viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Left;
                            }
                            else if(dataMessage[0] == "Right" &&
                                viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Left)
                            {
                                viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Right;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
        }

        public static int AddSnake()
        {
            ViewModelGames viewModelGamesPlayer = new ViewModelGames();
            viewModelGamesPlayer.SnakesPlayers = new Snakes()
            {
                points = new List<Snakes.Point>()
               {
                   new Snakes.Point() {X=30, Y = 10},
                   new Snakes.Point() {X=20, Y = 10},
                   new Snakes.Point() {X=10, Y = 10},
               },
                direction = Snakes.Direction.Start
            };
            viewModelGamesPlayer.Points = new Snakes.Point(new Random().Next(10, 783), new Random().Next(10, 410));
            viewModelGames.Add(viewModelGamesPlayer);
            return viewModelGames.FindIndex(x => x == viewModelGamesPlayer);
        }

        public static void Timer()
        {
            while (true)
            {
                Thread.Sleep(100);
                List<ViewModelGames> RemoteSnakes = viewModelGames.FindAll(x => x.SnakesPlayers.GameOver);

                if (RemoteSnakes.Count > 0)
                {
                    foreach (ViewModelGames DeadSnake in RemoteSnakes)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Отключил пользователя: {remoteIpAddress.Find(x => x.IdSnake == DeadSnake.IdSnake).IPAddress}" +
                            $": {remoteIpAddress.Find(x => x.IdSnake == DeadSnake.IdSnake).Port}");
                        remoteIpAddress.RemoveAll(x => x.IdSnake == DeadSnake.IdSnake);
                    }
                    viewModelGames.RemoveAll(x => x.SnakesPlayers.GameOver);
                }

                foreach (ViewModelUserSettings User in remoteIpAddress)
                {
                    Snakes Snake = viewModelGames.Find(x => x.IdSnake == User.IdSnake).SnakesPlayers;
                    for (int i = Snake.points.Count - 1; i >= 0; i--)
                    {
                        if (i != 0)
                        {
                            Snake.points[i] = Snake.points[i - 1];
                        }
                        else
                        {
                            int Speed = 10 + (int)Math.Round(Snake.points.Count / 20f);
                            if (Speed > MaxSpeed) Speed = MaxSpeed;
                            if (Snake.direction == Snakes.Direction.Right)
                            {
                                Snake.points[i] = new Snakes.Point() { X = Snake.points[i].X + Speed, Y = Snake.points[i].Y };
                            }
                            else if (Snake.direction == Snakes.Direction.Down)
                            {
                                Snake.points[i] = new Snakes.Point() { X = Snake.points[i].X, Y = Snake.points[i].Y + Speed };
                            }
                            else if (Snake.direction == Snakes.Direction.Up)
                            {
                                Snake.points[i] = new Snakes.Point() { X = Snake.points[i].X, Y = Snake.points[i].Y - Speed };
                            }
                            else if (Snake.direction == Snakes.Direction.Left)
                            {
                                Snake.points[i] = new Snakes.Point() { X = Snake.points[i].X - Speed, Y = Snake.points[i].Y };
                            }
                        }
                    }
                    if (Snake.points[0].X <= 0 || Snake.points[0].X >= 793)
                    {
                        Snake.GameOver = true;
                    } 
                    else if (Snake.points[0].Y <= 0 || Snake.points[0].Y >= 420)
                    {
                        Snake.GameOver = true;
                    }

                    if(Snake.direction != Snakes.Direction.Start)
                    {
                        for(int i = 1; i < Snake.points.Count; i++)
                        {
                            if (Snake.points[0].X > Snake.points[i].X - 1 && Snake.points[0].X <= Snake.points[i].X + 1)
                            {
                                if (Snake.points[0].Y > Snake.points[i].Y - 1 && Snake.points[0].Y <= Snake.points[i].Y + 1)
                                {
                                    Snake.GameOver = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (Snake.points[0].X >= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.X - 15 &&
                        Snake.points[0].X <= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.X + 15)
                    {
                        if (Snake.points[0].Y >= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.Y - 15 &&
                        Snake.points[0].Y <= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.Y + 15)
                        {
                            viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points = new Snakes.Point(
                                new Random().Next(10, 783),
                                new Random().Next(10, 410));

                            Snake.points.Add(new Snakes.Point()
                            {
                                X = Snake.points[Snake.points.Count - 1].X,
                                Y = Snake.points[Snake.points.Count - 1].Y
                            });

                            LoadLeaders();
                            Leaders.Add(new Leaders()
                            {
                                Name = User.Name,
                                Points = Snake.points.Count - 3
                            });
                            Leaders = Leaders.OrderByDescending(x => x.Points).ThenBy(x => x.Name).ToList();
                            viewModelGames.Find(x => x.IdSnake == User.IdSnake).Top =
                                Leaders.FindIndex(x => x.Points == Snake.points.Count - 3 && x.Name == User.Name) + 1;
                        }
                    }
                    if (Snake.GameOver)
                    {
                        LoadLeaders();
                        Leaders.Add(new Leaders()
                        {
                            Name = User.Name,
                            Points = Snake.points.Count - 3
                        });
                        SaveLeaders();
                    }
                }
                Send();
            }
        }

        public static void SaveLeaders()
        {
            string json = JsonConvert.SerializeObject(Leaders);
            StreamWriter SW = new StreamWriter("./leaders.txt");
            SW.WriteLine(json);
            SW.Close();
        }

        public static void LoadLeaders()
        {
            if (File.Exists("./leaders.txt"))
            {
                StreamReader SR = new StreamReader("./leaders.txt");
                string json = SR.ReadLine();
                SR.Close();
                if (!string.IsNullOrEmpty(json))
                {
                    Leaders = JsonConvert.DeserializeObject<List<Leaders>>(json);
                }
                else
                {
                    Leaders = new List<Leaders>();

                }
            }
            else
            {
                Leaders = new List<Leaders>();
            }
        }
    }
}
