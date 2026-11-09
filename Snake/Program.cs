using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Classes;

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
            //тенс
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
                    Console.WriteLine($"Отпраавил данные пользователю: {User.IPAddress}:{User.Port}")
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
    }
}
