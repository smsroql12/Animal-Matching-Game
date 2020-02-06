using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Media;
using System.IO;

namespace Puzzle
{
    public partial class PuzzleMain : Form
    {
        public PuzzleMain()
        {
            InitializeComponent();
        }
        static List<string> imgPath = new List<string>(); // 이미지 주소값이 들어 갈 배열
        static int[] clearBlock = new int[31]; // 이미 클리어 된 블럭들을 저장하는 배열 // 1~30 을 사용하며 클리어되면 블럭이 빠짐
        static int clearPoint = 15; // 한짝씩 맞출 때마다 1씩 감소하여 1이 되면 게임 끝
        static int clickCount = 0; // 클릭 카운트 값 (2가 되면 초기화)
        static int countDown = 1000; // 타이머 변수
        string[] StartHidden = { "GamePlay_Btn", "GameExit_Btn", "lbDeveloper", "picMainLogo" }; // 첫 화면에 표시 될 오브젝트들
        string first, second; // first : 첫번째 열어본 블럭, second : 두번째 열어본 블럭
        string dfirst, dsecond; // 맞춘 블럭을 disable 하기 위한 블럭 이름 저장 변수
        string resourcesPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\")) + "Resources\\"; // 리소스 파일 경로값 변수
        SoundPlayer sound = new SoundPlayer(Properties.Resources.Diring); // 맞췄을 경우 소리가 MediaPlayer에서 재생되지 않아 SoundPlayer로 재생
        private void PuzzleMain_Load(object sender, EventArgs e)
        {
            playSound(axWindowsMediaPlayer1, "MainScreen", true);
        }
        private void PuzzleMain_Resize(object sender, EventArgs e)
        {
            if (Width <= 630 || Width >= 630) Width = 630;
            if (Height <= 590 || Height >= 590 ) Height = 590;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (countDown == 0)    gameEnd(0);
            else
            {
                countDown = countDown - 1;
                progressBar1.Value = 1000 - countDown;
            }
        }
        private void GameExit_Btn_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void btnBack_Click(object sender, EventArgs e)
        {
            pnlGameResult.Visible = false;
            for (int i = 0; i < StartHidden.Length; i++)
                Controls[StartHidden[i]].Visible = true;
            playSound(axWindowsMediaPlayer1, "MainScreen", true);
        }
        private void GamePlay_Btn_Click(object sender, EventArgs e)
        {
            playSound(axWindowsMediaPlayer1, "InGame", true);
            pnlMainGame.Visible = true;
            int i;
            for(i = 0; i < StartHidden.Length; i++)
                Controls[StartHidden[i]].Visible = false;
            for (i = 1; i < 31; i++)
                pnlMainGame.Controls["block" + i].BackgroundImage = Image.FromFile(resourcesPath + "hide.png");
            for (i = 0; i <= 1; i++)
            {
                for (int j = 1; j <= 15; j++)
                    imgPath.Add(resourcesPath + j + ".jpg");
            }
            for (i = 0; i < imgPath.Count; i++)
                ShuffleList(imgPath);
            for (i = 0; i < 31; i++)
                clearBlock[i] = i;

            timer1.Start();
        }
        private void block1_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            string btnVal = btn.Name;
            btnVal = btnVal.Replace("block", "");
            pnlMainGame.Controls[btn.Name].BackgroundImage = Image.FromFile(imgPath[Convert.ToInt32(btnVal) - 1]); //동물 이미지로 변환
            if (!(clearPoint < 1))
            {
                if (clickCount == 0) // 두개중 한번 클릭
                {
                    playSound(axWindowsMediaPlayer2, "Reverse", false);
                    dfirst = btn.Name;
                    pnlMainGame.Controls[dfirst].Enabled = false; // 두번클릭 방지
                    first = Path.GetFileNameWithoutExtension(imgPath[Convert.ToInt32(btnVal) - 1]);
                    ++clickCount;
                }
                else // 두개중 두번 클릭
                {
                    playSound(axWindowsMediaPlayer3, "Reverse", false);
                    dsecond = btn.Name;
                    pnlMainGame.Controls[dsecond].Enabled = false; // 두번클릭 방지
                    second = Path.GetFileNameWithoutExtension(imgPath[Convert.ToInt32(btnVal) - 1]);
                    for (int i = 1; i < 31; i++)
                        pnlMainGame.Controls["block"+i].Enabled = false;
                    
                    if (first == second) // 맞췄을 경우
                    {
                        --clearPoint;
                        lbClearPoint.Text = "남은 개수 : " + clearPoint.ToString();
                        dfirst = dfirst.Replace("block", "");
                        dsecond = dsecond.Replace("block", "");
                        int d1 = Convert.ToInt32(dfirst);
                        int d2 = Convert.ToInt32(dsecond);
                        pnlMainGame.Controls["block" + d1].Enabled = false;
                        pnlMainGame.Controls["block" + d2].Enabled = false;
                        Array.Clear(clearBlock, d1, 1); // 배열 값을 0 으로 초기화 시킴.
                        Array.Clear(clearBlock, d2, 1);
                        sound.Play();
                        for (int i = 1; i <= clearBlock.Length -1; i++) // 1~30 까지 아직 맞추지 못한 블럭들이 clearBlock의 원소이니 clearBlock 원소들은 모두 enable
                            if (clearBlock[i] != 0) pnlMainGame.Controls["block" + clearBlock[i]].Enabled = true; // 배열 값이 0 이면 패스

                        if (clearPoint < 1) gameEnd(1); // 게임 클리어 체크
                    }
                    else //틀렸을 경우
                    {
                        Delay(500);
                        pnlMainGame.Controls[dfirst].BackgroundImage = Image.FromFile(resourcesPath + "hide.png");
                        pnlMainGame.Controls[dsecond].BackgroundImage = Image.FromFile(resourcesPath + "hide.png");

                        for (int i = 1; i <= clearBlock.Length - 1; i++)
                            if (clearBlock[i] != 0) pnlMainGame.Controls["block" + clearBlock[i]].Enabled = true;
                    }
                    clickCount = 0;
                }
            }
        }
        private void playSound(AxWMPLib.AxWindowsMediaPlayer player, string musicName, bool loop)
        {
            player.URL = resourcesPath + musicName + ".wav";
            player.Ctlcontrols.play();
            if (loop == true) player.settings.setMode("loop", true);
        }
        public static void ShuffleList<T>(List<T> list)
        {
            Random rnd = new Random();
            int random1;
            int random2;
            T tmp;
            for (int index = 0; index < list.Count; ++index)
            {
                random1 = rnd.Next(0, list.Count);
                random2 = rnd.Next(0, list.Count);
                tmp = list[random1];
                list[random1] = list[random2];
                list[random2] = tmp;
            }
        }
        private static DateTime Delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime AfterWards = ThisMoment.Add(duration);
            while (AfterWards >= ThisMoment)
            {
                Application.DoEvents();
                ThisMoment = DateTime.Now;
            }
            return DateTime.Now;
        }
        private void gameEnd(int r) // 게임 초기화
        {
            timer1.Stop();
            timer1.Dispose();
            countDown = 1000;
            progressBar1.Value = 0;
            for (int i = 0; i < StartHidden.Length; i++)
                Controls[StartHidden[i]].Visible = false;
            axWindowsMediaPlayer1.Ctlcontrols.stop();
            //r = 0 게임 오버, r = 1 게임 클리어
            if (r == 0) {
                picResult.Image = Image.FromFile(resourcesPath + "GameOver.png");
                playSound(axWindowsMediaPlayer4, "GameOver", false);
            }
            else {
                picResult.Image = Image.FromFile(resourcesPath + "GameClear.png");
                playSound(axWindowsMediaPlayer4, "GameWin", false);
            }
            pnlMainGame.Visible = false;
            pnlGameResult.Visible = true;
            imgPath.Clear();
            clearPoint = 15;
            clearBlock.Initialize();
            for (int i = 1; i < 31; i++)
                pnlMainGame.Controls["block" + i].Enabled = true;
            lbClearPoint.Text = "남은 개수 : " + clearPoint.ToString();
        }
    }
}