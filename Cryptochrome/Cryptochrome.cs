using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cryptochrome
{
    static class Cryptochrome
    {
        //хромосома или нулевая перестановка или массив символов или алфавит
        static readonly char[] chromRussian = { 'А', 'Б', 'В', 'Г', 'Д', 'Е', 'Ж', 'З', 'И', 'Й', 'К', 'Л', 'М', 'Н', 'О', 'П', 'Р', 'С', 'Т', 'У', 'Ф', 'Х', 'Ц', 'Ч', 'Ш', 'Щ', 'Ъ', 'Ы', 'Ь', 'Э', 'Ю', 'Я' };

        public static string[] Encrypt(string[] text)//зашифровка
        {
            char[] encryptionKey = Mutation((char[])chromRussian.Clone(), 1000);// перестановка букв хромосомы 1000 раз
            return Crypt(text, chromRussian, encryptionKey);//метод для защифровки с параметрами: text-массив строк или текст для зашифровки, chromRussian - хромосома, encryptionKey - порядок букв в зашифрованном алфавите-ключик
        }

        static char[] Mutation(char[] chromosome, int n)//случайная мутация, n-сколько раз мы переставляем
        {
            Random rand = new Random();
            for (int i = 0; i < n; i++)
            {
                //алгоритм перестановки двух букв хромосомы
                int a = rand.Next(chromosome.Length);
                int b = rand.Next(chromosome.Length);
                char tempChar = chromosome[a];
                chromosome[a] = chromosome[b];
                chromosome[b] = tempChar;
            }
            return chromosome;
        }

        static char[] SmartMutation(char[] chromosome, int problemLetter)//мутация2 для самой проблематичной буквы-один раз
        {
            Random rand = new Random();
            int a = problemLetter;
            int b = rand.Next(chromosome.Length);
            char tempChar = chromosome[a];
            chromosome[a] = chromosome[b];
            chromosome[b] = tempChar;
            return chromosome;
        }

        public static string[] Decrypt(string[] text, char[] encryptionKey)//расшифровка
        {
            return Crypt(text, encryptionKey, chromRussian);//идентична зашифровке,только последние параметры поменялись местами
        }

        static string[] Crypt(string[] text, char[] chromosome, char[] encryptionKey)
        {
            string chromosomeS = new string(chromosome);//переводим массив символов(алфавит) в строку: 'А','Б',...->АБВГДЕ...
            for (int j = 0; j < text.Length; j++)
            {
                text[j] = text[j].Replace("Ё", "Е");
                text[j] = text[j].Replace("ё", "е");
                for (int i = 0; i < chromosome.Length; i++)
                {
                    text[j] = text[j].Replace(chromosomeS.Substring(i, 1), chromosomeS.Substring(i, 1) + "№");//заменяем символ алфавита в тексте на такой же+спецсимвол: А->А№
                    text[j] = text[j].Replace(chromosomeS.Substring(i, 1).ToLower(), chromosomeS.Substring(i, 1).ToString().ToLower() + "№");//переводим в нижний регистр а->а№
                }
            }

            string encryptionKeyS = new string(encryptionKey);//encryptionKey-будущий ключ
            for (int j = 0; j < text.Length; j++)
            {
                for (int i = 0; i < chromosome.Length; i++)
                {
                    text[j] = text[j].Replace(chromosomeS.Substring(i, 1) + "№", encryptionKeyS.Substring(i, 1).ToString());
                    text[j] = text[j].Replace(chromosomeS.Substring(i, 1).ToLower() + "№", encryptionKeyS.Substring(i, 1).ToString().ToLower());
                }
            }

            return text;
        }

        public static Matrices MakeMatrices(string[] text)//делает матрицу по тексту
        {
            Matrices matrices = new Matrices(chromRussian);
            matrices.Fill(text);
            return matrices;
        }

        public static char[] FindEncryptionKey(Matrices matricesOfRussian, Matrices matricesOfEncrypted)//генетический алгоритм
        {
            //Initialization
            int sizeOfSmallPopulation = 20;//число родительских хромосом - число перестановок
            char[][] population = new char[sizeOfSmallPopulation * 3][];//вся популяция хромосом
            for (int i = 0; i < sizeOfSmallPopulation * 3; i++)
                population[i] = Mutation((char[])chromRussian.Clone(), 1000);//инициализация-случайная стартовая популяция

            //Genetic algorithm cycle
            float minEvaluation = 1;
            float oldMinEvaluation = 1;
            int efforts = 0;//количество попыток
            while (true)//бесконечный цикл
            {
                //Selection
                population = Selection(population, matricesOfRussian, matricesOfEncrypted, out minEvaluation, out char[] minEvaluationChrom);//убиваем неоптимальные хромосомы и сохраняем 2 лучших
                //Termination
                if (minEvaluation < 0.000001)//минимальная оценка - оптимальность - самое оптимальное - 0
                {
                    Console.WriteLine(new string(minEvaluationChrom) + "   0");
                    return minEvaluationChrom;
                }
                //Additional termination
                if (minEvaluation == oldMinEvaluation)//когда старое и новое совпали неоднократно, а 3000 раз подряд
                {
                    efforts++;
                    if (efforts > 2000)
                    {
                        Console.WriteLine(new string(minEvaluationChrom) + "   " + minEvaluation.ToString());
                        return minEvaluationChrom;
                    }
                }
                else
                {
                    efforts = 0;
                    oldMinEvaluation = minEvaluation;
                    Console.WriteLine(new string(minEvaluationChrom) + "   " + minEvaluation.ToString());//вывод
                }
                //Mutation
                /*for (int i = 0; i < sizeOfSmallPopulation; i++) population[i + sizeOfSmallPopulation] = SmartMutation((char[])population[i].Clone(), FindProblemLetter((char[])population[i].Clone(), matricesOfRussian, matricesOfEncrypted));*/
                Parallel.For(0, sizeOfSmallPopulation, (i, state) =>
                {
                    population[i + sizeOfSmallPopulation] = SmartMutation((char[])population[i].Clone(), FindProblemLetter((char[])population[i].Clone(), matricesOfRussian, matricesOfEncrypted));
                });//параллелим:мутируем 20 лучших от предыдущего шага и записываем в 20 следующих ячеек
                //Crossover                
                /*Random rand = new Random();
                for (int i = 0; i < sizeOfSmallPopulation; i++)
                {
                    char[][] chromsToCrossover = new char[3][];
                    chromsToCrossover[0] = population[i];
                    for (int j = 1; j < chromsToCrossover.Length; j++)
                        chromsToCrossover[j] = population[rand.Next(sizeOfSmallPopulation)];
                    population[i + sizeOfSmallPopulation * 2] = Crossover(chromsToCrossover);
                }*/
                Parallel.For(0, sizeOfSmallPopulation, (i, state) =>
                {
                    Random rand = new Random();
                    char[][] chromsToCrossover = new char[3][];
                    chromsToCrossover[0] = population[i];
                    for (int j = 1; j < chromsToCrossover.Length; j++)
                        chromsToCrossover[j] = population[rand.Next(sizeOfSmallPopulation)];
                    population[i + sizeOfSmallPopulation * 2] = Crossover(chromsToCrossover);
                });//скрещивание 3-x особей из 20 лучших
            }
        }

        static char[][] Selection(char[][] population, Matrices matricesOfRussian, Matrices matricesOfEncrypted, out float minEvaluation, out char[] minEvaluationChrom)//отбор самых лучших
        {
            //арена для отбора-метод турнира-выживает сильнейший, то есть более близкий по оптимальности к нулю
            float[] evaluations = new float[population.GetLength(0)];//матрицы оценок
            /*for (int i = 0; i < population.GetLength(0); i++)
                evaluations[i] = Evaluation(population[i], matricesOfRussian, matricesOfEncrypted);*/
            Parallel.For(0, population.GetLength(0), (i, state) =>
            {
                evaluations[i] = Evaluation(population[i], matricesOfRussian, matricesOfEncrypted);
            });//заполнение массива оценок для каждой хромосомы из популяции методом Evaluation

            Random rand = new Random();
            char[][] newPopulation = new char[population.GetLength(0)][];
            minEvaluation = 1;
            minEvaluationChrom = population[0];
            for (int i = 0; i * 3 < population.GetLength(0); i++)
            {
                //один i-ый и две случайных особи
                int a = i;
                int b = rand.Next(population.GetLength(0));
                int c = rand.Next(population.GetLength(0));
                //методом поиска самого минимального-самая маленькая оценка,близкая к нулю
                if (evaluations[a] <= evaluations[b] && evaluations[a] <= evaluations[c])
                {
                    newPopulation[i] = population[a];
                    if (evaluations[a] < minEvaluation) { minEvaluation = evaluations[a]; minEvaluationChrom = population[a]; }
                }
                else if (evaluations[b] <= evaluations[a] && evaluations[b] <= evaluations[c])
                {
                    newPopulation[i] = population[b];
                    if (evaluations[b] < minEvaluation) { minEvaluation = evaluations[b]; minEvaluationChrom = population[b]; }
                }
                else if (evaluations[c] <= evaluations[a] && evaluations[c] <= evaluations[b])
                {
                    newPopulation[i] = population[c];
                    if (evaluations[c] < minEvaluation) { minEvaluation = evaluations[c]; minEvaluationChrom = population[c]; }
                }
            }

            char[] previousChrom = newPopulation[0];
            for (int i = 1; i * 3 < newPopulation.GetLength(0); i++)
            {
                //if (newPopulation[i] == previousChrom) newPopulation[i] = Mutation((char[])newPopulation[i].Clone(), 1);
                if (newPopulation[i] == previousChrom) newPopulation[i] = SmartMutation((char[])newPopulation[i].Clone(), FindProblemLetter((char[])newPopulation[i].Clone(), matricesOfRussian, matricesOfEncrypted));
                else previousChrom = newPopulation[i];
            }//дополнительная мутация

            return newPopulation;
        }

        static float Evaluation(char[] chromosome, Matrices matricesOfRussian, Matrices matricesOfEncrypted)//метод наименьших квадратов
        {
            float level0LeastSquares = 0;
            for (int i = 0; i < matricesOfRussian.level0.Length; i++)
            {
                matricesOfRussian.letterNumbers.TryGetValue(chromosome[i], out int letterNumber);
                level0LeastSquares += (matricesOfRussian.level0[i] - matricesOfEncrypted.level0[letterNumber])
                                    * (matricesOfRussian.level0[i] - matricesOfEncrypted.level0[letterNumber]);
            }
            level0LeastSquares /= matricesOfRussian.level0.Length;

            float level1LeastSquares = 0;
            for (int i = 0; i < matricesOfRussian.level0.Length; i++)
                for (int j = 0; j < matricesOfRussian.level0.Length; j++)
                {
                    matricesOfRussian.letterNumbers.TryGetValue(chromosome[i], out int letterNumberI);
                    matricesOfRussian.letterNumbers.TryGetValue(chromosome[j], out int letterNumberJ);
                    level1LeastSquares += (matricesOfRussian.level1[i, j] - matricesOfEncrypted.level1[letterNumberI, letterNumberJ])
                                        * (matricesOfRussian.level1[i, j] - matricesOfEncrypted.level1[letterNumberI, letterNumberJ]);
                }
            level1LeastSquares /= matricesOfRussian.level0.Length * matricesOfRussian.level0.Length;

            float level2LeastSquares = 0;
            for (int i = 0; i < matricesOfRussian.level0.Length; i++)
                for (int j = 0; j < matricesOfRussian.level0.Length; j++)
                    for (int k = 0; k < matricesOfRussian.level0.Length; k++)
                    {
                        matricesOfRussian.letterNumbers.TryGetValue(chromosome[i], out int letterNumberI);
                        matricesOfRussian.letterNumbers.TryGetValue(chromosome[j], out int letterNumberJ);
                        matricesOfRussian.letterNumbers.TryGetValue(chromosome[k], out int letterNumberK);
                        level2LeastSquares += (matricesOfRussian.level2[i, j, k] - matricesOfEncrypted.level2[letterNumberI, letterNumberJ, letterNumberK])
                                            * (matricesOfRussian.level2[i, j, k] - matricesOfEncrypted.level2[letterNumberI, letterNumberJ, letterNumberK]);
                    }
            level2LeastSquares /= matricesOfRussian.level0.Length * matricesOfRussian.level0.Length * matricesOfRussian.level0.Length;

            return (level0LeastSquares + level1LeastSquares + level2LeastSquares) / 3f;
        }

        static int FindProblemLetter(char[] chromosome, Matrices matricesOfRussian, Matrices matricesOfEncrypted)//ищем проблемную букву
        {
            float maxlevel0LeastSquares = 0;
            int problemLetter = 0;
            for (int i = 0; i < matricesOfRussian.level0.Length; i++)
            {
                matricesOfRussian.letterNumbers.TryGetValue(chromosome[i], out int letterNumber);
                float level0LeastSquares = (matricesOfRussian.level0[i] - matricesOfEncrypted.level0[letterNumber])
                                         * (matricesOfRussian.level0[i] - matricesOfEncrypted.level0[letterNumber]);
                if (level0LeastSquares > maxlevel0LeastSquares)
                {
                    maxlevel0LeastSquares = level0LeastSquares;
                    problemLetter = i;
                }
            }
            return problemLetter;
        }

        static char[] Crossover(char[][] chromosomes)//скрещивание
        {
            Random rand = new Random();
            char[] newChrom = new char[chromosomes[0].Length];
            Dictionary<char, int> letters = new Dictionary<char, int>(chromosomes[0].Length);
            int[] chekGens = new int[chromosomes[0].Length];
            int chekGensNumber = 0;
            for (int i = 0; i < chromosomes[0].Length; i++)
            {
                bool tryAgain = true;
                int efforts = 0;
                while (tryAgain)
                {
                    if (efforts < 30)
                    {
                        char letter = chromosomes[rand.Next(chromosomes.GetLength(0))][i];
                        tryAgain = letters.ContainsKey(letter);
                        efforts++;
                        if (!tryAgain)
                        {
                            newChrom[i] = letter;
                            letters.Add(letter, 0);
                        }
                    }
                    else
                    {
                        tryAgain = false;
                        chekGens[chekGensNumber] = i;
                        chekGensNumber++;
                    }
                }
            }

            for (int i = 0; i < chekGensNumber; i++)
            {
                bool tryAgain = true;
                while (tryAgain)
                {
                    char letter = chromosomes[rand.Next(chromosomes.GetLength(0))][rand.Next(chromosomes[0].Length)];
                    tryAgain = letters.ContainsKey(letter);
                    if (!tryAgain)
                    {
                        newChrom[chekGens[i]] = letter;
                        letters.Add(letter, 0);
                    }
                }
            }

            return newChrom;
        }

        public static void SaveEncryptionKey(string path, char[] encryptionKey)
        {
            string[] encryptionKeyS = { new string(encryptionKey) };
            File.WriteAllLines(path, encryptionKeyS);
        }

        public static char[] LoadEncryptionKey(string path)
        {
            string[] encryptionKeyS = File.ReadAllLines(path);
            return encryptionKeyS[0].ToCharArray();
        }
    }
}