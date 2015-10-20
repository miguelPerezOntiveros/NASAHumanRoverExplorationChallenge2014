using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

using System.IO;
using System.Windows;
using System.Windows.Media;
using Microsoft.Kinect;
using System.IO.Ports;
using MySql.Data.MySqlClient;
using System.Data;
using System.Threading;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    class serialQuery
    {
        SerialPort sp;
        int[] buffer = new int[60];
        bool yainit;
        float vib, temp, accX, accY, accZ;
        int p1, p2, p3, p4, cnt;
        String vel, lon, lat, dire, queryStr, aux;
        Client c;

        public serialQuery(SerialPort sp, Client c)
        {
            this.c = c;
            this.sp = sp;
            vib = temp = accX = accY = accZ = cnt = 0;
            vel = lon = lat = dire = "";
        }
        public void Init()
        {
            sp.ReadExisting();
            read(60);
            read(60);

            p1 = sp.ReadByte();
            p2 = sp.ReadByte();
            p3 = sp.ReadByte();
            p4 = sp.ReadByte();

            if (p1 == 0 && p2 >= 18 && p2 <= 63 && p3 == 1 && p4 == 0)
                yainit = true;

            while (!yainit)
            {
                p1 = p2;
                p2 = p3;
                p3 = p4;
                p4 = sp.ReadByte();
                Console.WriteLine(p1 + "\t" + p2 + "\t" + p3 + "\t" + p4 + "\t");

                if (p1 == 0 && p2 >= 18 && p2 <= 63 && p3 == 1 && p4 == 0)
                    yainit = true;
            }

            read(54);
        }

        public void Update()
        {

            Thread myThread = new System.Threading.Thread(delegate()
            {

                while (true)
                {
                    if (!yainit) Init();

                    try
                    {
                        lee();
                        queryStr = "INSERT INTO entradas(vibrations,temperature,accX,accY,accZ,speed,longitude,latitude,direction)VALUES('" + vib + "','" + temp + "','" + accX + "','" + accY + "','" + accZ + "','" + vel + "','" + lon + "','" + lat + "','" + dire + "')";
                        c.query(queryStr);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Error en Update() " + e.Message);
                    }
                }

            });
            myThread.Start();

        }

        public int read(int cant)
        {
            for (int i = 0; i < cant; i++)
            {
                buffer[i] = (int)sp.ReadByte();
                cnt++;
            }
            return cant;
        }

        public void lee()
        {
            cnt = 0;

            Console.WriteLine("\nComienzo de lecura. cnt= " + cnt);
            read(3);
            Console.WriteLine("vib:" + (vib = (buffer[0] + (256 * buffer[1])) * ((buffer[2] * (-2)) + 1)));
            read(3);
            Console.WriteLine("temp:" + (temp = (buffer[0] + (256 * buffer[1])) * ((buffer[2] * (-2)) + 1)));
            read(2);
            Console.WriteLine("accX:" + (accX = (buffer[0]) * (buffer[1] * (-2) + 1)));
            read(2);
            Console.WriteLine("accY:" + (accY = (buffer[0]) * (buffer[1] * (-2) + 1)));
            read(2);
            Console.WriteLine("accZ:" + (accZ = (buffer[0]) * (buffer[1] * (-2) + 1)));

            aux = "";
            p1 = 0;
            do
            {
                aux += (char)p1;
                p1 = sp.ReadByte();
                cnt++;
            }
            while ((char)p1 != '*');
            Console.WriteLine("vel:" + (vel = aux));

            aux = "";
            p1 = 0;
            do
            {
                if (p1 != 0)
                    aux += (char)p1;
                p1 = sp.ReadByte();
                cnt++;
            }
            while ((char)p1 != '*');
            Console.WriteLine("lon:" + (lon = aux));

            aux = "";
            p1 = 0;
            do
            {
                if (p1 != 0)
                    aux += (char)p1;
                p1 = sp.ReadByte();
                cnt++;
            }
            while ((char)p1 != '#');
            Console.WriteLine("lat:" + (lat = aux));


            while (cnt < 57)
            {
                sp.ReadByte();
                cnt++;
            }

            p1 = sp.ReadByte();
            p2 = sp.ReadByte();
            p3 = sp.ReadByte();
            cnt += 3;
            aux = p1 + p2 + p3 + "";
            Console.WriteLine("dire:" + (dire = aux));

            Console.WriteLine("Fin de lectura. cnt= " + cnt + "\n");
        }
    }
}
