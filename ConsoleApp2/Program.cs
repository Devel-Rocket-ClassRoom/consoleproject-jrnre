using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;




namespace ConsoleApp2
{
    public class SaveLoadJson
    {
        // GameData를 Json으로 저장하는 테스트 함수
        static public void SaveGameData(char[][] m)
        {
            // 저장할 파일 경로
            string folderPath = "./GameData";
            string filePath = Path.Combine(folderPath, "data.json");  // 폴더와 파일 이름 합치기

            // 폴더가 존재하지 않을 경우 생성
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // 직렬화
            string result = JsonSerializer.Serialize(m, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, result);

            // 테스트 출력
            Console.WriteLine(result);
        }

        // Json으로 저장된 GameData를 읽는 테스트 함수
        //public void LoadGameData()
        //{
        //    string folderPath = "./GameData";
        //    string filePath = Path.Combine(folderPath, "data.json");  // 폴더와 파일 이름 합치기

        //    // 역직렬화
        //    string s = File.ReadAllText(filePath);
        //    GameData mm = JsonSerializer.Deserialize<GameData>(s);

        //    if (mm != null)
        //    {
        //        Console.WriteLine("읽기 성공!: " + mm);
        //    }
        //    else
        //    {
        //        Console.WriteLine("정상적인 데이터가 아닙니다.");
        //    }
        //}

        // 2차원 맵을 char[][]로 바꾸는 함수
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
        public int row;
        public int col;
        public class GameData
        {
            // Json에 포함
            [JsonInclude] private string stageName;
            [JsonInclude] private int dungeonCount;

            // 포함하지 않음
            [JsonIgnore] public int Count { get; set; }

            public GameData(string name, int dCount, int count)
            {
                stageName = name;
                dungeonCount = dCount;
                Count = count;
            }
        }

        public void PlayGame()
        {
            // TODO: () 스테이지가 여러개일 경우 개선

            Console.WriteLine("맵의 가로크기와 세로크기를 입력하세요 (예: 10 15)");
            string line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) line = "10 15";

            var input = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            int row = 10;
            int col = 15;
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

                // 맵 저장 테스트
                // 맵을 미리 만들어서 저장

                game.PlayGame(md.mapData, md.monsterCount, stageLevel);
                stageLevel++;
                Console.WriteLine("축하합니다! 게임종료!");
                if (stageLevel == 6) break;
            }
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

    public class MapData : Map
    {
        public char[,] mapData;
        public int monsterCount;

        public void CreateMap(int rows, int cols)
        {
            char[,] map = new char[,]
            {
                { '#', '#', '#', '#', '#', '#', '#', '#', '#', },
                { '#', '#', '#', '#', '#', '#', '#', '#', '#', },
                { '#', '#', '#', '#', '#', '#', '#', '#', '#', }
            };
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

        public void PlayGame(char[,] mapData, int mCount, int stageLevel)
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

                Console.WriteLine("이동(W, A, S, D):");

                ConsoleKeyInfo keyInfo = Console.ReadKey();
                string cmd = keyInfo.KeyChar.ToString().ToUpper();

                int nextY = playerY;
                int nextX = playerX;

                if (cmd == "A") nextX--;
                else if (cmd == "D") nextX++;
                else if (cmd == "W") nextY--;
                else if (cmd == "S") nextY++;

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