using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Cryptochrome
{
    [Serializable] 
    class Matrices
    {
        public float[] level0;//одиночные буквы
        public float[,] level1;//пары букв
        public float[,,] level2;//тройки букв
        public readonly Dictionary<char, int> letterNumbers;//словарик

        public Matrices(char[] chromosome) //конструктор класса
        {
            //матрицы вероятностей
            level0 = new float[chromosome.Length];//массив длины 32
            level1 = new float[chromosome.Length, chromosome.Length];//матрицы размера 32 на 32
            level2 = new float[chromosome.Length, chromosome.Length, chromosome.Length];//32 на 32 на 32 
            letterNumbers = new Dictionary<char, int>(chromosome.Length);
            for (int i = 0; i < chromosome.Length; i++)
                letterNumbers.Add(chromosome[i], i); //заполнение словарика
        }

        public void Fill(string[] text)//массив строчек
        {
            //матрицы частоты встречаемости
            int[] level0Mass = new int[level0.Length];//хранят число заданных одиночных букв в тексте 
            int[,] level1Mass = new int[level0.Length, level0.Length];//хранят число пар букв в тексте - число биграмм
            int[,,] level2Mass = new int[level0.Length, level0.Length, level0.Length];//хранят число троек букв в тексте - число триграмм

            for (int i = 0; i < text.Length; i++)//цикл строчек всего текста
            {
                text[i] = text[i].ToUpper();//переводим буквы текста в заглавные
                for (int j = 0; j < text[i].Length; j++)//цикл для конкретного символа строки
                {
                    char currChar;
                    char.TryParse(text[i].Substring(j, 1), out currChar);
                    if (letterNumbers.TryGetValue(currChar, out int letterNumber0))
                    {
                        level0Mass[letterNumber0]++;//записываем букву и её порядковый номер в словаре

                        if (j + 1 < text[i].Length)//для того,чтобы проверить,что следующий символ существует
                        {
                            char.TryParse(text[i].Substring(j + 1, 1), out currChar);//по аналогии с одиночными буквами
                            if (letterNumbers.TryGetValue(currChar, out int letterNumber1))
                            {
                                level1Mass[letterNumber0, letterNumber1]++;

                                if (j + 2 < text[i].Length)//проверка того,что существует третий символ 
                                {
                                    char.TryParse(text[i].Substring(j + 2, 1), out currChar);// по аналогии с биграммами
                                    if (letterNumbers.TryGetValue(currChar, out int letterNumber2))
                                        level2Mass[letterNumber0, letterNumber1, letterNumber2]++;
                                }
                            }
                        }
                    }
                }
            }

            int level0MassSum= 0, level1MassSum = 0, level2MassSum = 0;

            for (int i = 0; i < level0.Length; i++)//общее количество всех одиночных букв, биграмм и триграмм
            {
                level0MassSum += level0Mass[i];
                for (int j = 0; j < level0.Length; j++)
                {
                    level1MassSum += level1Mass[i, j];
                    for (int k = 0; k < level0.Length; k++)
                        level2MassSum += level2Mass[i, j, k];
                }
            }

            for (int i = 0; i < level0.Length; i++)//ищем вероятности
            {
                level0[i] = level0Mass[i] / (float)level0MassSum;
                for (int j = 0; j < level0.Length; j++)
                {
                    level1[i, j] = level1Mass[i, j] / (float)level1MassSum;
                    for (int k = 0; k < level0.Length; k++)
                        level2[i, j, k] = level2Mass[i, j, k] / (float)level2MassSum;
                }
            }
        }

        public void Save(string path)//сохраняем все матрицы в бинарные файлы
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, this);
            }
        }

        public static Matrices Load(string path)//загружаем из бинарного файла
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (Matrices)formatter.Deserialize(fs);
            }
        }
    }
}