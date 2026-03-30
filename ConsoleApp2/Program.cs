using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace ConsoleApp2
{
    public class SaveLoadJson
    {
        static public void SaveGameData(char[][] m)
        {
            string folderPath = "./GameData";
            string filePath = Path.Combine(folderPath, "data.json");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string result = JsonSerializer.Serialize(m, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, result);
        }

        static public char[][] ConvertMap(char[,] map)
        {
            int rows = map.GetLength(0);
            int cols = map.GetLength(1);
            char[][] result = new char[rows][];

            for (int i = 0; i < rows; i++)
            {
                result[i] = new char[cols];
                for (int j = 0; j < cols; j++)
                {
                    result[i][j] = map[i, j];
                }
            }
            return result;
        }
    }

    public class DungeonGame
    {
        public void PlayGame()
        {
            Console.WriteLine("맵의 가로크기와 세로크기를 입력하세요 (예: 10 15)");
            string line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) line = "10 15";

            var input = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            int row = 10;
            int col = 15;
            // 예외처리
            if (input.Length >= 2 && int.TryParse(input[0], out int r) && int.TryParse(input[1], out int c))
            {
                row = Math.Max(3, r);
                col = Math.Max(3, c);
            }

            int stageLevel = 1;
            while (stageLevel < 6)
            {
                MapData md = new MapData();
                md.CreateMap(row, col);

                var m = SaveLoadJson.ConvertMap(md.mapData);
                SaveLoadJson.SaveGameData(m);

                Map game = new Map();
                bool cleared = game.PlayGame(md, stageLevel);

                if (cleared)
                {
                    Console.Clear();
                    game.PrintMap(md.mapData);
                    Console.WriteLine($"\n{stageLevel}스테이지 클리어!");
                    stageLevel++;
                    Thread.Sleep(1000);
                }

                if (stageLevel == 6)
                {
                    Console.WriteLine("모든 스테이지 클리어! 게임 종료.");
                    break;
                }
            }
        }
    }

    public class Player : Character { protected override void MakeSound() { } }
    public class Monster : Character, IMonster
    {
        protected override void MakeSound() { }
        public void MakeDyingSound() { }
        public string NickName { get; set; }
    }
    public class Dragon : Monster { }

    public abstract class Character
    {
        public string Name { get; set; }
        abstract protected void MakeSound();
    }

    public interface IMonster
    {
        void MakeDyingSound();
        string NickName { get; set; }
    }

    public class MapData
    {
        public char[,] mapData;
        public int monsterCount;

        public void CreateMap(int rows, int cols)
        {
            Random rand = new Random();
            mapData = new char[rows, cols];
            monsterCount = 0;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (r == 0 || r == rows - 1 || c == 0 || c == cols - 1)
                        mapData[r, c] = '#';
                    else
                        mapData[r, c] = ' ';
                }
            }
            mapData[1, 1] = 'P';

            int area = (rows - 2) * (cols - 2);
            int targetMonsters = Math.Min(5, Math.Max(1, area / 30));

            for (int i = 0; i < targetMonsters; i++)
            {
                int mr, mc, attempts = 0;
                do
                {
                    mr = rand.Next(1, rows - 1);
                    mc = rand.Next(1, cols - 1);
                    attempts++;
                } while (mapData[mr, mc] != ' ' && attempts < 500);

                if (mapData[mr, mc] == ' ')
                {
                    mapData[mr, mc] = 'M';
                    monsterCount++;
                }
            }

            int maxStacles = Math.Max(0, area / 8);
            int block = 0, allAttempts = 0;
            while (block < maxStacles && allAttempts < maxStacles * 20 + 500)
            {
                int orow = rand.Next(1, rows - 1);
                int ocol = rand.Next(1, cols - 1);
                allAttempts++;
                if (mapData[orow, ocol] == ' ') { mapData[orow, ocol] = '#'; block++; }
            }
        }
    }

    public class Map
    {
        public int playerY;
        public int playerX;

        public void PrintMap(char[,] data)
        {
            for (int i = 0; i < data.GetLength(0); i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                    Console.Write(data[i, j]);
                Console.WriteLine();
            }
        }

        public bool PlayGame(MapData md, int stageLevel)
        {
            playerY = 1;
            playerX = 1;
            string statusMessage = "";

            while (true)
            {
                Console.Clear();
                Console.WriteLine($"=== 현재 스테이지: {stageLevel} ===");
                PrintMap(md.mapData);
                Console.WriteLine($">> 남은 몬스터: {md.monsterCount}");

                if (!string.IsNullOrEmpty(statusMessage))
                {
                    Console.WriteLine($">> {statusMessage}");
                    statusMessage = "";
                }

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                string cmd = keyInfo.KeyChar.ToString().ToUpper();

                //가상 좌표
                int nextY = playerY;
                int nextX = playerX;

                if (cmd == "A") nextX--;
                else if (cmd == "D") nextX++;
                else if (cmd == "W") nextY--;
                else if (cmd == "S") nextY++;
                else continue;

                if (md.mapData[nextY, nextX] == '#')
                {
                    statusMessage = "벽입니다!";
                }
                else
                {
                    if (md.mapData[nextY, nextX] == 'M')
                    {
                        md.monsterCount--;
                        statusMessage = "몬스터를 잡았습니다!";
                    }

                    md.mapData[playerY, playerX] = ' ';
                    playerY = nextY;
                    playerX = nextX;
                    md.mapData[playerY, playerX] = 'P';
                }

                if (md.monsterCount <= 0) return true;
            }
        }

        public static void Main()
        {
            DungeonGame game = new DungeonGame();
            game.PlayGame();
        }
    }
}
//// 몬스터 포지션 말고 카운트로 -1 개수 줄어들면 맵 넘어가게 설계
//// instance 인스턴스(실체, 실제 데이타, 실제 객체)
//// -공통 데이터, 메소드 선언

//// ct + K D
//// ci cd
