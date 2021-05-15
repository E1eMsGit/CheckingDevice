using System;

namespace TK158.Helpers
{
    static class TestsDataTA1004M1
    {
        /// <summary>
        /// Преобразование строки данных (hex) в массив байт.
        /// </summary>
        /// <param name="data">Данные из файла или с тестовой View.</param>
        /// <returns>Данные из файла в виде массива.</returns>
        public static byte[] GetArrayFromHexString(string data)
        {
            if (data == string.Empty)
            {
                return null;
            }

            string[] strData = data.Trim(' ').Split(' ');
            byte[] dataForSend = new byte[strData.Length];
 
            for (int bytesCounter = 0; bytesCounter < strData.Length; bytesCounter++)
            {
                dataForSend[bytesCounter] = Convert.ToByte(strData[bytesCounter], 16);
            }

            return dataForSend;
        }       
        /// <summary>
        /// Подготовка данных для проверки в режимах "Входы информации".
        /// </summary>
        /// <param name="address1">Адрес первого устройства.</param>
        /// <param name="address2">Адрес второго устройства.</param>
        /// <param name="address3">Адрес третьего устройства.</param>
        /// <param name="informationLength">Длина посылки.</param>
        public static byte[] GetTestData1(string address1, string address2, string address3, byte informationLength)
        {
            byte[] testData = new byte[informationLength];

            byte data = (byte)(Convert.ToByte(address1, 2) << 4);
            byte dataWithPhasing = (byte)(data | Constants.PHASING);           

            testData[0] = dataWithPhasing;

            for (int i = 1; i < 5; i++)
            {
                testData[i] = data;
            }

            testData[8] = data;

            data = (byte)(Convert.ToByte(address2, 2) << 4);
            dataWithPhasing = (byte)(data | Constants.PHASING);

            testData[5] = dataWithPhasing;
            testData[7] = data;

            data = (byte)(Convert.ToByte(address3, 2) << 4);
            testData[6] = data;


            for (int i = 9; i < informationLength; i += 9)
            {
                for (int j = 0; j < 9; j++)
                {
                    testData[i + j] = testData[j];
                }
            }

            return testData;
        }
        /// <summary>
        /// Подготовка данных для проверки в режимах "Формирование ЗИ и Тоср", "Проверка прохождения информации".
        /// </summary>
        /// <param name="address1">Адрес первого устройства.</param>
        /// <param name="address2">Адрес второго устройства.</param>
        /// <param name="address3">Адрес третьего устройства.</param>
        /// <param name="informationLength">Длина посылки.</param>
        public static byte[] GetTestData2(string address1, string address2, string address3, byte informationLength)
        {
            byte[] testData = new byte[informationLength];

            byte data = (byte)(Convert.ToByte(address1, 2) << 4);
            byte dataWithPhasing = (byte)(data | Constants.PHASING);

            testData[0] =  dataWithPhasing;

            for (int i = 1; i < 6; i++)
            {
                testData[i] = data;
            }

            data = (byte)(Convert.ToByte(address2, 2) << 4);
            dataWithPhasing = (byte)(data | Constants.PHASING);

            testData[6] = dataWithPhasing;
            testData[7] = data;

            data = (byte)(Convert.ToByte(address3, 2) << 4);
            dataWithPhasing = (byte)(data | Constants.PHASING);

            testData[8] = dataWithPhasing;
            testData[9] = data;

            for (int i = 10; i < informationLength; i += 10)
            {
                for (int j = 0; j < 10; j++)
                {
                    testData[i + j] = testData[j];
                }
            }

            return testData;
        }
        /// <summary>
        /// Подготовка данных для проверки в режимах 
        /// "Задержка выдачи на АУ1", "Измерение параметров сигналов ЗИ1 и ЗИ2", "Измерение параметров сигналов Тоср", "Разряды кода".
        /// </summary>
        /// <param name="address1">Адрес первого устройства.</param>
        /// <param name="informationLength">Длина посылки.</param>
        /// <param name="phasing">Фазировка (для режима "Измерение параметров сигналов Тоср").</param>
        public static byte[] GetTestData3(string address1, int informationLength, bool phasing=false)
        {
            byte[] testData = new byte[informationLength];

            byte data = (byte)(Convert.ToByte(address1, 2) << 4);

            if (phasing)
            {
                data = (byte)(data | Constants.PHASING);
            }

            for (int i = 0; i < informationLength; i++)
            {
                testData[i] = data;
            }

            return testData;
        }
        /// <summary>
        /// Подготовка данных для проверки в режимах "Задержка выдачи на АУ2", "Измерение параметров сигналов ЗИ3 и ЗИ4".
        /// </summary>
        /// <param name="address2">Адрес второго устройства.</param>
        /// <param name="informationLength">Длина посылки.</param>
        public static byte[] GetTestData4(string address2, int informationLength)
        {
            byte[] testData = new byte[informationLength];

            byte data = (byte)(Convert.ToByte(address2, 2) << 4);
            byte dataWithPhasing = (byte)(data | Constants.PHASING);

            for (int i = 0; i < informationLength; i += 3)
            {
                testData[i] = dataWithPhasing;
                testData[i + 1] = data;
                testData[i + 2] = data;
            }

            return testData;
        }
        /// <summary>
        /// Подготовка данных для проверки в режимах "Задержка выдачи на АУ3", "Измерение параметров сигнала ЗИ5".
        /// </summary>
        /// <param name="address3">Адрес третьего устройства.</param>
        /// <param name="informationLength">Длина посылки.</param>
        public static byte[] GetTestData5(string address3, int informationLength)
        {
            byte[] testData = new byte[informationLength];

            byte data = (byte)(Convert.ToByte(address3, 2) << 4);
            byte dataWithPhasing = (byte)(data | Constants.PHASING);

            for (int i = 0; i < informationLength; i += 2)
            {
                testData[i] = dataWithPhasing;
                testData[i + 1] = data;
            }

            return testData;
        }
        /// <summary>
        /// Подготовка данных для проверки в режимах "Проверка тока потребления".
        /// </summary>
        /// <param name="informationLength">Длина посылки.</param>
        public static byte[] GetTestData6(int informationLength)
        {
            byte[] testData = new byte[informationLength];

            for (int i = 0; i < informationLength; i++)
            {
                testData[i] = Constants.PHASING;
            }

            return testData;
        }
        /// <summary>
        /// Подготовка данных для проверки в режимах "Проверка адресатора".
        /// </summary>
        /// <param name="informationLength">Длина посылки.</param>
        public static byte[] GetTestData7(int informationLength)
        {
            byte[] testData = new byte[informationLength];

            for (int i = 0; i < informationLength; i += 16)
            {
                for (int j = 0; j < 16; j++)
                {
                    testData[i + j] = (byte)((j << 4) | Constants.PHASING);
                }
            }

            return testData;
        }
    }
}
