using System;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleApp2
{
    public class DungeonGame
    {
        public int row;
        public int col;
        public void PlayGame()
        {
            // TODO: () 스테이지가 여러개일 경우 개선

            Console.WriteLine("맵의 가로크기와 세로크기를 입력하세요 (예: 10 15)");
            string line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) line = "10 15";

            var input = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            row = 10;
            col = 15;
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
                Map game = new Map();
                game.PlayGame(md.mapData, md.monsterCount,stageLevel);
                stageLevel++;
                Console.WriteLine("축하합니다! 게임종료!");
                if (stageLevel == 6) break;
            }
        }
        public class Player : Character
        {
            override protected void MakeSound() { }
        }

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
                        mc = mc = rand.Next(1, cols - 1);
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

            public void PlayGame(char[,] mapData, int mCount,int stageLevel)
            {
                playerY = 1;
                playerX = 1;
                
                int MonsterCount = mCount;
                
                string statusMessage = "";

                while (true)
                {
                    Console.Clear();
                    Console.WriteLine($"=== 현재 스테이지: {stageLevel} ===");
                    PrintMap(mapData);
                    Console.WriteLine($">> 남은 몬스터: {MonsterCount}");

                    if (!string.IsNullOrEmpty(statusMessage))
                    {
                        Console.WriteLine($">> {statusMessage}");
                        statusMessage = "";
                    }

                    Console.WriteLine("이동(L, R, U, D):");
                    string cmd = Console.ReadLine().ToUpper();

                    int nextY = playerY;
                    int nextX = playerX;
                    

                    if (cmd == "L") nextX--;
                    else if (cmd == "R") nextX++;
                    else if (cmd == "U") nextY--;
                    else if (cmd == "D") nextY++;
                    
                    else continue;

                    if (mapData[nextY, nextX] == '#')
                    {
                        statusMessage = "벽입니다!";
                    }
                    else
                    {
                        if (mapData[nextY, nextX] == 'M')
                        {
                            MonsterCount--;
                            statusMessage = "몬스터를 잡았습니다!";
                        }
                        

                        mapData[playerY, playerX] = ' ';
                        playerY = nextY;
                        playerX = nextX;
                        
                        mapData[playerY, playerX] = 'P';
                        
                    }
                    if (MonsterCount <= 0)
                    {
                        Console.Clear();
                        PrintMap(mapData);
                        Console.Write($"{stageLevel++}스테이지 클리어!");
                        if (stageLevel == 6)
                        {
                            Console.WriteLine("");
                        }
                        Thread.Sleep(1000);
                        return;
                    }
                }
            }
        }
        public static void Main()
        {
            DungeonGame game = new DungeonGame();
            game.PlayGame();
        }
    }
}
// 몬스터 포지션 말고 카운트로 -1 개수 줄어들면 맵 넘어가게 설계
// instance 인스턴스(실체, 실제 데이타, 실제 객체)
// -공통 데이터, 메소드 선언

// 던전게임
// 1. 부모 클래스 (플레이어 몬스터 공통) - 공통 데이터, 메소드 선언
// 2. 플레이어, 몬스터 클래스 정의
// -  각각 전용 데이터 선언
// 3. 플레이어 전용 데이터, 메소드(함수)
// -  예) 이동시 함수 사용
// 4. 몬스터 전용 데이터, 메소드
// ----------------------------------------------------------------
// 5. 맵 클래스 만들고 코드 정리

// ct + K D
// ci cd