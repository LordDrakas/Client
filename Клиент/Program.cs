using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

namespace Клиент
{

    class Program
    {

        static void sendcomand(string userName,out string message,out int comand)
        {
            comand = -1;
            string type = "kill";
            string find = "kill";
            int kind = -1;
            // ввод сообщения
            Console.WriteLine("Введите индекс команды:");
            Console.WriteLine("1 - отправить файл отчета");
            Console.WriteLine("2 - Выполнить поиск");
            message = Console.ReadLine();
            comand = Convert.ToInt16(message);
            switch (comand)
            {
                case 1:
                    {
                        Console.WriteLine("Введите тип отчета:");
                        Console.WriteLine("1 - Общий");
                        Console.WriteLine("2 - Отладка");
                        Console.WriteLine("3 - Авария");
                        type = Console.ReadLine();
                        comand = Convert.ToInt32(message);
                        
                                             
                        break;
                    }
                case 2:
                    {
                        Console.WriteLine("Введите вид поиска");
                        Console.WriteLine("1 - по пользователю");
                        Console.WriteLine("2 - по типу отчета");
                        Console.WriteLine("3 - по дате");
                        
                        kind = Convert.ToInt16(Console.ReadLine());
                        switch(kind)
                        {
                            case 1:
                                {
                                    Console.WriteLine("Введите имя пользователя, соблюдая регистр");
                                    break;
                                }
                            case 2:
                                {
                                    Console.WriteLine("Введите тип отчета:");
                                    Console.WriteLine("1 - Общий");
                                    Console.WriteLine("2 - Отладка");
                                    Console.WriteLine("3 - Авария");
                                    break;
                                }
                            case 3:
                                {
                                    Console.WriteLine("Введите дату в формате - 00.00.0000");
                                    break;
                                }
                            
                        }
                        
                        find = Console.ReadLine();

                        break;
                    }

            }
            message = String.Format("{0}*{1}*{2}*{3}*{4}", userName, message, type, find, kind);
            

        }
        static void SendFile(NetworkStream stream)
        {
            BinaryFormatter format = new BinaryFormatter();
            byte[] buf = new byte[1024];
            int count;
            FileStream fs = new FileStream("1.txt", FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            long k = fs.Length;//Размер файла.
            format.Serialize(stream, k.ToString());//Вначале передаём размер
            while ((count = br.Read(buf, 0, 1024)) > 0)
            {
                format.Serialize(stream, buf);//А теперь в цикле по 1024 байта передаём файл
            }
            Console.WriteLine("Файл отчета отправлен");
            fs.Close();
            br.Close();
        }
        static void Reseive(NetworkStream stream)
        {
            Console.WriteLine("ОТВЕТ СЕРВЕРА:");
            while (true)
            {
                // получаем ответ
                byte[] data1 = new byte[64]; // буфер для получаемых данных
                StringBuilder builder1 = new StringBuilder();
                int bytes1 = 0;
                do
                {

                    bytes1 = stream.Read(data1, 0, data1.Length);
                    builder1.Append(Encoding.Unicode.GetString(data1, 0, bytes1));
                }
                while (stream.DataAvailable);

                string message1 = builder1.ToString();
                if (message1 == "") break;
                Console.WriteLine(message1.Replace("***",""));
                if ((message1.Contains("Доступ") == true)|| (message1.Contains("***") == true)) break;
                //if (message1 == " ") Console.WriteLine(" ");
            }
        }
        static void LogIn(NetworkStream stream, string user, string pass)
        {
            
            string message = String.Format("{0}*{1}", user, pass);
            // преобразуем сообщение в массив байтов
            byte[] data = Encoding.Unicode.GetBytes(message);
            // отправка сообщения
            stream.Write(data, 0, data.Length);
            

        }
        const int port = 8888;
        const string address = "127.0.0.1";

        static void Main(string[] args)
        {

            Console.Write("Введите имя пользователя, соблюдая регистр:");
            string userName = Console.ReadLine();
            Console.Write("Введите пароль:");
            string password = Console.ReadLine();
            TcpClient client = null;
            int comand = -1;
            
            try
            {
                byte[] data;//
                client = new TcpClient(address, port);
                NetworkStream stream = client.GetStream();
                LogIn(stream, userName, password);
                Reseive(stream);
                Console.ReadLine();
                Console.Clear();
                while (true)
                {
                    string comsend = "";
                    sendcomand(userName, out comsend, out comand);
                    // преобразуем сообщение в массив байтов
                    data = Encoding.Unicode.GetBytes(comsend);
                    // отправка сообщения
                    stream.Write(data, 0, data.Length);

                    switch (comand)
                    {
                        case 1:
                            {
                                SendFile(stream);
                                break;
                            }
                        case 2:
                            {

                                Reseive(stream);

                                break;
                            }

                    }
                    Console.ReadLine();
                    Console.Clear();
                }
               
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.ReadLine();
                client.Close();
            }
        }
        
    }
}
