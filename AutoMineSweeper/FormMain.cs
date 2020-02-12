using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using CShapDM;

namespace AutoMineSweeper
{
    public partial class FormMain : Form
    {
        CDmSoft dm = new CDmSoft();
        private IntPtr _dm = IntPtr.Zero;
        public FormMain()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            int Handle = GetHandle();
           // ShowDebugInfo(Handle.ToString());
            dm.BindWindowEx(Handle, "gdi", "windows", "windows", "", 0);
            // chức năng findstr của dm . add dict vào , cái này rất hay .
            dm.SetDict(0, "dm_soft.txt");
            this.timer.Enabled = true;
            this.txtInfo.Text = "";

            if (chkAutoRestart.Checked)
            {
                StartGame();
            }
        }

        private void StartGame()
        {
            GameState gameState = GetGameState();

            if (gameState != GameState.Ready)
            {
                Thread.Sleep(1000);
                dm.KeyPress(113);
                Thread.Sleep(1000);
                Recognize();
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            this.timer.Enabled = false;
        }

        private void btnDebug_Click(object sender, EventArgs e)
        {
            //phần này để test 
            int Handle = GetHandle();
            ShowDebugInfo(Handle.ToString());
        }

           

        private void timer_Tick(object sender, EventArgs e)
        {
            Recognize();
        }

        private int GetHandle()
        {
          
            return dm.FindWindow("", "Minesweeper"); ; 
        }

        private GameState GetGameState()
        {
            
            GameState gameState = GameState.Ready;
            //test findstr
            string temp = dm.FindStrFastEx(0, 0, 2000, 2000,"ready|thua|win", "000000", 0.9);
            //giải thích thêm phần này : tìm 3  chữ ở trên ( do mình làm 3 hình ảnh icon thành chữ (làm dict ) ví dụ tìm được chữ nào thì trả lại kết quá ID đó ví dụ 0 là ready
           // tuy nhiên nếu có 3 icon thì cũng trả 3 kết quả và tọa độ ví dụ có 2 3 icoin thì trả kết quá 0(100,200)|1(200,100)|2(100,100), nhưng cái này thì không
           //thể vì icon này ko xuất hiện đồng thời, làm chức năng này test dict .
            if (temp.Length > 0 )
            {
            //ShowDebugInfo("\r\n"+ temp);
            int checkstage = Int32.Parse(temp.Substring(0, 1));
              if (checkstage == 1)
               {
                    gameState = GameState.Fail;
                ShowDebugInfo("\r\nGame fail!");
                 
              }
            if (checkstage == 0)
            {
                gameState = GameState.Ready;
             //  ShowDebugInfo("\r\nGame ready");
            }
            if (checkstage == 2)
            {
                gameState = GameState.Success;
               ShowDebugInfo("\r\nGame Success");
            }
            }
            return gameState;
        }

        private void Recognize()
        {
            //Không đổi
          

            //Nhận trạng thái trò chơi 
            GameState gameState = GetGameState();

            //Trạng thái không phải trò chơi
            if (gameState != GameState.Ready)
            {
                ShowDebugInfo("\r\nGame over!");
               
               
                if (chkAutoRestart.Checked)
                {
                    StartGame();
                }
                else {
                    this.timer.Enabled = false;
                    return; }
            }


            //Xác định ví trí của trò chơi
            //Chiều ngang max ô là :
            int MaxX = 30;
            //Chiều dọc max ô là :
            int MaxY = 16;
            //chu vi  :
            int RectWidth = 16;
            // Tọa độ ô đầu tiên bên trái
            int left = 12;
            // Tọa độ ô đầu tiên bên  trên
            int top = 55;
            // chiều ngang bao nhiêu ô( tính pixel )
            int width = 16;
            // chiều dọc có bao nhiêu ô ( tính pixel )
           int height = 16;
            // vị trí trung tâm ô đầu tiên 
            int centerleft = 20;
            int centertop = 63;


            // bỏ ảnh vào ô
            this.pictureBox1.Image?.Dispose();
            dm.FreePic("info.bmp");

            dm.DeleteFile("info.bmp");
            dm.Capture(14, 56, 495, 313, "info.bmp");

            this.pictureBox1.Image = Image.FromFile("info.bmp");
            //

            //Xác định ma trận

            MineState[,] MineMap = new MineState[MaxX, MaxY];

            for (int x = 0; x < MaxX; x++)
            {
                for (int y = 0; y < MaxY; y++)
                {
                

                    // unknow là chưa khám phá , do trung tâm trùng màu trắng với ô None ( ô không có gì ) nên để lệch pixel
                    if (dm.CmpColor(12 + x * width, 63 + y * height, "ffffffff", 1.0) == 0) { MineMap[x, y] = MineState.Unknow; }

                    if (dm.CmpColor(20 + x * width, 63 + y * height, "0000ff", 1.0) == 0) { MineMap[x, y] = MineState.One; }
                    if (dm.CmpColor(20 + x * width, 63 + y * height, "008000", 1.0) == 0) { MineMap[x, y] = MineState.Two; }
                    if (dm.CmpColor(20 + x * width, 63 + y * height, "ff0000", 1.0) == 0) { MineMap[x, y] = MineState.Three; }
                    if (dm.CmpColor(20 + x * width, 63 + y * height, "000080", 1.0) == 0) { MineMap[x, y] = MineState.Four; }
                    if (dm.CmpColor(20 + x * width, 63 + y * height, "800000", 1.0) == 0) { MineMap[x, y] = MineState.Five; }
                    if (dm.CmpColor(20 + x * width, 63 + y * height, "008080", 1.0) == 0) { MineMap[x, y] = MineState.Six; }
                    // chưa rảnh làm , chia 2 thằng này do nó trùng với mine nên gây lỗi 
                 //   if (dm.CmpColor(22 + x * width, 63 + y * height, "000000" , 1.0) == 0) { MineMap[x, y] = MineState.Seven; }
                  //  if (dm.CmpColor(20 + x * width, 63 + y * height, "808080", 1.0) == 0) { MineMap[x, y] = MineState.Eight; }
                    if (dm.CmpColor(20 + x * width, 63 + y * height, "000000", 1.0)  == 0) { MineMap[x, y] = MineState.Mine; }
                    //tương tự unknow tìm pixel trong ô cho nó có đặc trưng riêng
                    if (dm.CmpColor(12 + x * width, 63 + y * height, "c0c0c0", 1.0) == 0) { MineMap[x, y] = MineState.None; }
                
                }
            }

            //Vẽ vào ô - thấy thừa quá nên bỏ 
            //Bitmap bitmapDebug = new Bitmap(255, 185);
            //using (Graphics graphics = Graphics.FromImage(bitmapDebug))
            //{
            //    string s = "";

            //    for (int x = 0; x < MaxX; x++)
            //        for (int y = 0; y < MaxY; y++)
            //        {
            //            switch (MineMap[x, y])
            //            {
            //                case MineState.Unknow: s = "?"; break;
            //                case MineState.None: s = "0"; break;
            //                case MineState.One: s = "1"; break;
            //                case MineState.Two: s = "2"; break;
            //                case MineState.Three: s = "3"; break;
            //                case MineState.Four: s = "4"; break;
            //                case MineState.Five: s = "5"; break;
            //                case MineState.Six: s = "6"; break;
            //                case MineState.Seven: s = "7"; break;
            //                case MineState.Eight: s = "8"; break;
            //                case MineState.Mine: s = "9"; break;
            //                default: s = "X"; break;
            //            }
            //            graphics.DrawString(s, new Font("Arial", 8), Brushes.Black, x * 8, y * 11);
            //        }

            //    this.pictureBox2.Image?.Dispose();
            //    this.pictureBox2.Image = bitmapDebug;
            //}

            List<ClickEvent> clickEventList = new List<ClickEvent>();
            bool FindGoodPoint = false;   //Nếu tìm thấy một điểm thích hợp, việc tính toán được hoàn thành


            //Bước 1: Thuật toán cơ bản ... 
          //  Debug.WriteLine("1 thuật toán cơ bản ...");
            for (int x = 0; x < MaxX; x++)
            {
                for (int y = 0; y < MaxY; y++)
                {
                    int MineCount = (int)MineMap[x, y];

                    if (MineCount >= 1)
                    {
                        List<BoxLocation> unknowBoxs = SearchMines(x, y, MaxX, MaxY, MineState.Unknow, MineMap);    //Chưa mở gần đó
                        List<BoxLocation> MineBoxs = SearchMines(x, y, MaxX, MaxY, MineState.Mine, MineMap);        //mỏ  xác định

                        if (unknowBoxs.Count > 0)
                        {
                            //Thuật toán chính 1: 
                            if (MineBoxs.Count == MineCount)
                            {
                                clickEventList.Clear();
                                foreach (var box in unknowBoxs)
                                {
                                    clickEventList.Add(new ClickEvent(box.LocationX, box.LocationY, ClickType.LeftClick));
                                }
                                FindGoodPoint = true;
                                break;
                            }

                            //Thuật toán chính 2: 
                            if (MineBoxs.Count + unknowBoxs.Count == MineCount)
                            {
                                clickEventList.Clear();
                                foreach (var box in unknowBoxs)
                                {
                                    clickEventList.Add(new ClickEvent(box.LocationX, box.LocationY, ClickType.RightClick));
                                }
                                FindGoodPoint = true;
                                break;
                            }
                        }
                    }
                }

                if (FindGoodPoint)
                {
                    break;
                }
            }


            //Bước 2: Bổ sung thuật toán ..... Lần đầu tiên không tính được điểm phù hợp, hãy sử dụng thuật toán phức tạp để tính lại.
            if (!FindGoodPoint)
            {
              
                //Tính toán
                List<UnknowBoxSum> UnknowBoxSumList = new List<UnknowBoxSum>();

                for (int locx = 0; locx < MaxX; locx++)
                {
                    for (int locy = 0; locy < MaxY; locy++)
                    {
                        int MineCount = (int)MineMap[locx, locy];

                        if (MineCount >= 1 && MineCount <= 6)
                        {
                            List<BoxLocation> unknowBoxs = SearchMines(locx, locy, MaxX, MaxY, MineState.Unknow, MineMap);    //Chưa mở gần đó
                            List<BoxLocation> MineBoxs = SearchMines(locx, locy, MaxX, MaxY, MineState.Mine, MineMap);        //Mỏ đã được xác định

                            if (unknowBoxs.Count >= 2)
                            {
                                UnknowBoxSum unknowBoxSum = new UnknowBoxSum();
                                unknowBoxSum.Boxes = unknowBoxs;
                                unknowBoxSum.Sum = MineCount - MineBoxs.Count;
                                UnknowBoxSumList.Add(unknowBoxSum);
                            }
                        }
                    }
                }

            //    Debug.WriteLine($"UnknowBoxSumList={UnknowBoxSumList.Count}");

                if (UnknowBoxSumList.Count > 0)
                {

                    //Trận đấu
                    for (int locx = 0; locx < MaxX; locx++)
                    {
                        for (int locy = 0; locy < MaxY; locy++)
                        {
                            int MineCount = (int)MineMap[locx, locy];

                            if (MineCount >= 1 && MineCount <= 8)
                            {
                                List<BoxLocation> unknowBoxs = SearchMines(locx, locy, MaxX, MaxY, MineState.Unknow, MineMap);    //Chưa mở gần đó
                                List<BoxLocation> MineBoxs = SearchMines(locx, locy, MaxX, MaxY, MineState.Mine, MineMap);        //Mỏ đã được xác định

                                if (unknowBoxs.Count >= 2)
                                {
                                    foreach (var UnknowBoxSum in UnknowBoxSumList)
                                    {
                                        if (unknowBoxs.Count - UnknowBoxSum.Boxes.Count == 1)
                                        {
                                            if (UnknowBoxSum.MatchBox(unknowBoxs))   //Đầu tiên xác định xem nó có khớp không
                                            {
                                                //Số mìn  == Tổng: không phải mìn
                                                if ((MineCount - MineBoxs.Count - UnknowBoxSum.Sum) == 0)
                                                {

                                                  
                                                    BoxLocation box = UnknowBoxSum.GetNotBelongBox(unknowBoxs);

                       //                             Debug.WriteLine($"Match（Không có bơm ）:({locx},{locy})->({box.LocationX},{box.LocationY})");

                                                    clickEventList.Add(new ClickEvent(box.LocationX, box.LocationY, ClickType.LeftClick));


                                                    FindGoodPoint = true;
                                                    break;
                                                }

                                                //Số giảm trừ-Sum = 1:
                                                if ((MineCount - MineBoxs.Count - UnknowBoxSum.Sum) == 1)
                                                {

                                                    //Đưa ra
                                                    BoxLocation box = UnknowBoxSum.GetNotBelongBox(unknowBoxs);
                                            //        Debug.WriteLine($"Match（á bơm ）:({locx},{locy})->({box.LocationX},{box.LocationY})");

                                                    clickEventList.Add(new ClickEvent(box.LocationX, box.LocationY, ClickType.RightClick));

                                                    FindGoodPoint = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (FindGoodPoint)
                            {
                                break;
                            }
                        }

                        if (FindGoodPoint)
                        {
                            break;
                        }
                    }

                }
            }


            //Bước 3:  không tìm thấy một vị trí phù hợp, chỉ có thể mở ngẫu nhiên
            if (!FindGoodPoint)
            {
            //    Debug.WriteLine("3Mở ngẫu nhiên...");

                float[,] ProbabilityMap = new float[MaxX, MaxY];

                //Tính xác suất tối thiểu
                float MinProbability = 1;

                for (int x = 0; x < MaxX; x++)
                {
                    for (int y = 0; y < MaxY; y++)
                    {
                        if (MineMap[x, y] == MineState.Unknow)
                        {
                            ProbabilityMap[x, y] = CalcProbability(x, y, MaxX, MaxY, MineMap);

                            if (ProbabilityMap[x, y] < MinProbability)
                            {
                                MinProbability = ProbabilityMap[x, y];
                            }
                        }
                    }
                }
             //   Debug.WriteLine($"MinProbability={MinProbability}");

                //Lấy điểm với xác suất nhỏ nhất
                for (int x = 0; x < MaxX; x++)
                {
                    for (int y = 0; y < MaxY; y++)
                    {
                        if (MineMap[x, y] == MineState.Unknow && Math.Abs(ProbabilityMap[x, y] - MinProbability) < 0.001)
                        {
                            clickEventList.Add(new ClickEvent(x, y, ClickType.LeftClick));
                        }
                    }
                }

                /*
                foreach (var clickEvent in clickEventList)
                {
                    Debug.WriteLine($"selected clickEvent:{clickEvent}");
                }
                */

                int Count = clickEventList.Count;
                //ShowDebugInfo($"\r\nRandom Select:Count={Count}");

                if (Count == 0)
                {
                    //ShowDebugInfo("\r\n clickEventList.Count == 0,I can't do anything!");
                    this.timer.Enabled = false;
                    return;
                }
                else
                {
                    if (Count == 1)
                    {
                        ClickEvent clickEvent = clickEventList[0];
                    }
                    else
                    {
                        Random random = new Random();
                        ClickEvent clickEvent = clickEventList[random.Next(Count)];
                        clickEventList.Clear();
                        clickEventList.Add(clickEvent);
                    }
                }
            }


            // Click 
            ClickScreen(RectWidth, left, top, centerleft, centertop, clickEventList);
        }


        //Tìm  hộp phụ kiện 
        private List<BoxLocation> SearchMines(int locx, int locy, int MaxX, int MaxY, MineState mineType, MineState[,] mineMaps)
        {
            List<Point> points = new List<Point>();
            points.Add(new Point(locx - 1, locy - 1));
            points.Add(new Point(locx - 1, locy));
            points.Add(new Point(locx - 1, locy + 1));
            points.Add(new Point(locx, locy - 1));
            points.Add(new Point(locx, locy + 1));
            points.Add(new Point(locx + 1, locy - 1));
            points.Add(new Point(locx + 1, locy));
            points.Add(new Point(locx + 1, locy + 1));

            List<BoxLocation> mines = new List<BoxLocation>();
            foreach (Point p in points)
            {
                int x = p.X;
                int y = p.Y;

                if (x >= 0 && x < MaxX && y >= 0 && y < MaxY && mineMaps[x, y] == mineType)
                {
                    mines.Add(new BoxLocation(x, y, mineType));
                }
            }

            return mines;
        }

        //Tính xác suất
        private float CalcProbability(int locx, int locy, int MaxX, int MaxY, MineState[,] MineMap)
        {
            //Debug.WriteLine($"Probability:({locx},{locy}):Begin");

            float AverageProbability = 0.21f;    //Xác suất trung bình: xác suất nhấp trong một khoảng trống (dữ liệu thực nghiệm, chọn 0,21 hoặc 0,19, tùy thuộc vào độ khó)                       

            float Probability;

            if (MatchMinesType(locx, locy, MaxX, MaxY, MineMap))
            {
                Probability = AverageProbability;
            }
            else
            {
                Probability = 0.1f;

                List<Point> points = new List<Point>();
                points.Add(new Point(locx - 1, locy - 1));
                points.Add(new Point(locx - 1, locy));
                points.Add(new Point(locx - 1, locy + 1));
                points.Add(new Point(locx, locy - 1));
                points.Add(new Point(locx, locy + 1));
                points.Add(new Point(locx + 1, locy - 1));
                points.Add(new Point(locx + 1, locy));
                points.Add(new Point(locx + 1, locy + 1));

                foreach (Point p in points)
                {
                    int x = p.X;
                    int y = p.Y;

                    if (x >= 0 && x < MaxX && y >= 0 && y < MaxY)
                    {
                        int mines = (int)MineMap[x, y];
                        if (mines >= 1 && mines <= 8)
                        {
                            int flags = SearchMines(x, y, MaxX, MaxY, MineState.Mine, MineMap).Count;
                            int unkows = SearchMines(x, y, MaxX, MaxY, MineState.Unknow, MineMap).Count;
                            float currentProbability = (float)(mines - flags) / unkows;

                            if (currentProbability > Probability)
                            {
                                Probability = currentProbability;
                            }

                           Debug.WriteLine($"({x},{y}):mines={mines},flags={flags},unkows={unkows},currentProbability={currentProbability}");
                        }
                    }
                }
            }

          

            return Probability;
        }

        //Xác định nếu không có hộp có số
        private Boolean MatchMinesType(int locx, int locy, int MaxX, int MaxY, MineState[,] mineMaps)
        {
            bool match = true;

            List<Point> points = new List<Point>();
            points.Add(new Point(locx - 1, locy - 1));
            points.Add(new Point(locx - 1, locy));
            points.Add(new Point(locx - 1, locy + 1));
            points.Add(new Point(locx, locy - 1));
            points.Add(new Point(locx, locy + 1));
            points.Add(new Point(locx + 1, locy - 1));
            points.Add(new Point(locx + 1, locy));
            points.Add(new Point(locx + 1, locy + 1));

            foreach (Point p in points)
            {
                int x = p.X;
                int y = p.Y;

                if (x >= 0 && x < MaxX && y >= 0 && y < MaxY && mineMaps[x, y] != MineState.Unknow && mineMaps[x, y] != MineState.Mine)
                {
                    match = false;
                }
            }

            return match;
        }

        //Click
        private void ClickScreen(int RectWidth, int left, int top, int centerleft, int centertop, List<ClickEvent> clickEventList)
        {
            foreach (var clickEvent in clickEventList)
            {
               
             
                int clickPointX = 20 + clickEvent.LocalX *16 ;
                int clickPointY = 63 + clickEvent.LocalY *16;

                dm.MoveTo(clickPointX, clickPointY);
               

                if (clickEvent.ClickType == ClickType.LeftClick)
                {
                    dm.LeftClick();
                }
                else
                {
                    dm.RightClick();
                }
            }
        }

        private void ShowDebugInfo(string info)
        {
            //Debug.WriteLine(info);
            this.txtInfo.Text += "\r\n" + info;
        }
    }
}
