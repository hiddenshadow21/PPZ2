namespace PlatformKeyboardControl
{
    internal class KeyboardControll
    {
        public float Height_MM { get; set; } = 120;
        public float Right { get; set; }
        public float Front { get; set; }
        public ushort Velocity { get; set; }

        private bool _shouldRun = true;

        public void Test()
        {
            // See https://aka.ms/new-console-template for more information


            var platforma = MotionSystemsPlatform.GetAnInstanceOfThePlatform();
            platforma.SetWorkingMode(true); // trzeba podać argument bool, w zależności od trybu, w jakim chce się pracować

            platforma.StartThePlatform();
            int x = 0;

            var sinus = new float[360]; // ostatecznie jest tam teraz wpisany sygnał trójkątny
                                        // float maxxx = 0.331613f;
                                        // float maxxx = 167.0f;
            float maxxx = 1.0f;
            for (int i = 0; i < 360; i++) // przygotowywanie od razu całej tablicy danych dla położeń, zwykle docelowe położenia liczy/pozyskuje się tuż przed wysyłaniem (np. co klatkę) żądań do platformy, ale ten kod jest tylko przykładem
            {
                // sinus[i] = (float)(0.2*Math.Sin(i * Math.PI / 180));

                if (i < 90) sinus[i] = maxxx * i / 90.0f;
                else if (i < 180) sinus[i] = maxxx * (180 - i) / 90.0f;
                else if (i < 270) sinus[i] = maxxx * (180 - i) / 90.0f;
                else sinus[i] = maxxx * (i - 4 * 90) / 90.0f;
            }

            ushort v = 65535; // v_platformSpeed_ushort_od_0_do_65535

            Task.Run(() =>
            {
                while (_shouldRun)
                {
                    var key = Console.ReadKey().Key;

                    HandleKey(key);
                }
            });

            try
            {
                while (_shouldRun)
                {
                    Thread.Sleep(9);
                    /*platforma.OrderThePlatformToMoveBasedOnTelemetry(
                                0.5f * (float)(0.2 * Math.Sin(x * Math.PI / 180)), // bodyAngularVelocityYaw
                                0.0f, // bodyAngularVelocityPitch
                                6.0f * (float)(0.2 * Math.Sin(x * Math.PI / 180)), // bodyAngularVelocityRoll
                                8f * (float)(0.2 * Math.Sin(x * Math.PI / 180)), // bodyLinearAccelerationForward
                                0.0f, // bodyLinearAccelerationUpward
                                1.0f * (float)(0.2 * Math.Sin(x * Math.PI / 180)), // bodyLinearAccelerationRight
                                0.0f, // extraRotationYaw
                                0.8f * sinus[x], // extraRotationPitch
                                0.0f, // extraRotationRoll
                                0.0f, // extraTranslationRight
                                0.0f, // extraTranslationUpward
                                0.0f // extra
                                );*/

                    platforma.OrderThePlatformToReachNewPosition(v, Height_MM, Front, Right);

                    x++;
                    if (x == 360)
                        x = 0;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                platforma.StopThePlatform();
            }
        }

        void HandleKey(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    Front+=0.05f;
                    break;
                case ConsoleKey.DownArrow:
                    Front-=0.05f;
                    break;
                case ConsoleKey.LeftArrow:
                    Right-=0.05f;
                    break;
                case ConsoleKey.RightArrow:
                    Right+=0.05f;
                    break;
                case ConsoleKey.Z:
                    Height_MM+=10;
                    break;
                case ConsoleKey.X:
                    Height_MM-=10;
                    break;
                case ConsoleKey.Q:
                    _shouldRun = false;
                    break;
                default:
                    break;
            }
        }

    }
}
