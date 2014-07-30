using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Animation;


namespace WpfPomodoro
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        // タイマー
        DispatcherTimer dispatcherTimer;

        // 時計のイベントハンドラー
        EventHandler clockHandler;
        // タイマーのイベントハンドラー
        EventHandler timerHandler;


        //フォームの状態のステータス
        enum FormStatus { STOP, START }
        //タイマーの状態のステータス
        enum TimerStaus { WORK, BREAK }

        // ワークタイム
        private TimeKeeper _worktimer;
        //ブレイクタイム
        private TimeKeeper _breaktimer;

        FormStatus _formStatus;
        TimerStaus _timerStatus;

        // フォーム状態変更イベント
        event Action<FormStatus> FormStatusUpdate;
        //タイマー状態変更イベント
        event Action<TimerStaus> TimerStatusUpdate;

        // フォーム状態のプロパティ
        private FormStatus formStatus
        {
            set
            {
                if (_formStatus != value) {
                    _formStatus = value;
                    if (FormStatusUpdate != null)
                    {

                        FormStatusUpdate(value);

                    }
                }


            }
            get { return _formStatus; }
        }

        //タイマー状態のプロパティ
        private TimerStaus timerStatus
        {
            set
            {
                if (_timerStatus != value)
                {
                    _timerStatus = value;
                    if (TimerStatusUpdate != null)
                    {
                        TimerStatusUpdate(value);
                    }
                }

            }
            get { return _timerStatus; }
        }

        Boolean IsBreak { get{ return timerStatus == TimerStaus.BREAK; }}
        Boolean IsWork { get{ return timerStatus == TimerStaus.WORK; }}
        Boolean IsStart { get{ return formStatus == FormStatus.START; }}
        Boolean IsStop { get { return formStatus == FormStatus.STOP; } }

   
        //画面切り替えアニメーション
        Storyboard greenStroyBoard;
        Storyboard redStroryBoard;

        public MainWindow()
        {
            InitializeComponent();

            //タイマーの初期化
            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0,0,Properties.Settings.Default.BreakTime);
            // ハンドラーの初期化
            clockHandler = new EventHandler(Clock_Tick);
            timerHandler = new EventHandler(Timer_Tick);
            
            

            //フォームのハンドラー
            FormStatusUpdate += formStatus => { ButtonChange(); TimerStatusChange(); };

            //タイマー状態のハンドラー
            TimerStatusUpdate += timerStatus => { ButtonChange(); TimerTypeChange(); };

           // 時計のハンドラーを設定
            dispatcherTimer.Tick += clockHandler;

            //アニメーション
            greenStroyBoard = (Storyboard)FindResource("GreenStoryboard");
            redStroryBoard = (Storyboard)FindResource("RedStoryboard");


    

            // タイマーを作る
            this.WorkTimer = new TimeKeeper(Properties.Settings.Default.WorkTime * 1000);
            this.BreakTimer = new TimeKeeper(Properties.Settings.Default.BreakTime * 1000);
            this.formStatus = FormStatus.STOP;
            this.timerStatus = TimerStaus.WORK;

            //表示の設定
            ButtonChange();
            TimerTypeChange();
            TimerDisp();

            // ディスパッチタイマーのスタート
            dispatcherTimer.Start();
        }

        private TimeKeeper WorkTimer
        {
            set
            {
                // それぞれの更新イベントでそれぞれ TextControl を更新
                value.UpdateMax += timekeeper => { TimerDisp(); };
                value.UpdateCount += timekeeper => { TimerDisp(); };
                value.TimerStop += timekeeper => { timerSwitch(); };
                _worktimer = value;
            }
            get 
            {
                return _worktimer;
            }

        }

        private TimeKeeper BreakTimer
        {
            set
            {
                // それぞれの更新イベントでそれぞれ TextControl を更新
                value.UpdateMax += timekeeper => { TimerDisp(); };
                value.UpdateCount += timekeeper => { TimerDisp(); };
                value.TimerStop += timekeeper => { timerSwitch(); };
                _breaktimer = value;
            }
            get
            {
                return _breaktimer;
            }
        }



        // 時計のハンドラー
        void Clock_Tick(object sender, EventArgs e)
        {
            ClockBox.Text = DateTime.Now.ToString("F");
        }

        // タイマーのハンドラー
        public void Timer_Tick(object sender, EventArgs e)
        {
            if (timerStatus == TimerStaus.WORK)
            {
               this.WorkTimer.Count += Properties.Settings.Default.BreakTime;
            }
            else if (timerStatus == TimerStaus.BREAK)
            {
                this.BreakTimer.Count += Properties.Settings.Default.BreakTime;
            }

        }


        // タイマーの状態の変更
        private void TimerStatusChange()
        {
            //タイマー
            if (IsStart)
            {
                dispatcherTimer.Tick += timerHandler;
            }else if (IsStop) {
                dispatcherTimer.Tick -= timerHandler;
            }
   
        }

        // タイマーの種類の変更
        private void TimerTypeChange()
        {
            //タイマー
            if (IsWork)
            {
                StatusBox.Text = Properties.Resources.WORKING;
                AnnonceText.Visibility = System.Windows.Visibility.Hidden;
                redStroryBoard.Begin();
               
            }
            else if (IsBreak)
            {
                StatusBox.Text = Properties.Resources.BREAKING;
                AnnonceText.Visibility = System.Windows.Visibility.Visible;
                greenStroyBoard.Begin();
            }

        }


        // タイマースタート
        private void timerStart()
        {

            formStatus = FormStatus.START;
        }

        //タイマーストップ
        private void timerStop()
        {

            formStatus = FormStatus.STOP;
        }

        //タイマーのリスタート
        private void timerRestart()
        {
            this.WorkTimer.Reset();
            this.BreakTimer.Reset();

            timerStatus = TimerStaus.WORK;
            formStatus = FormStatus.START;
   
        }

        // タイマーの切り替え
        private void timerSwitch()
        {
            if (timerStatus == TimerStaus.WORK)
            {
                timerStatus = TimerStaus.BREAK;
            }
            else if (timerStatus == TimerStaus.BREAK)
            {
                timerStatus = TimerStaus.WORK;
                timerStop();
                timerRestart();
            }
        }

        // 左ボタン文字列の表記
        private String LeftButtonString()
        {
            if (IsBreak)
            {
                return Properties.Resources.RESTART;
            }else if (IsWork) {
                if (IsStart)
                {
                    return Properties.Resources.STOP;
                }
                else if (IsStop)
                {
                    return Properties.Resources.WORK;
                }
            }

            return Properties.Resources.ERROR;
        }

        // 右ボタン文字列の表記
        private String RightButtonString()
        {
            if (IsWork)
            {
                return Properties.Resources.BREAK;
            }
            else if (IsBreak)
            {
                if (IsStart)
                {
                    return Properties.Resources.STOP;
                }
                else if (IsStop)
                {
                    return Properties.Resources.BREAK;
                }
            }

            return Properties.Resources.ERROR;
        }

        // ボタン状態の変更
        public void ButtonChange()
        {
            LeftButton.Content = LeftButtonString();
            RightButton.Content = RightButtonString();
        }

        //タイマー表記
        private void TimerDisp()
        {
            int time = 0;

            if (IsWork)
            {
                time = this.WorkTimer.TimeLeft();
            }
            else if (IsBreak)
            {
                time = this.BreakTimer.TimeLeft();
            }

            time = time / 1000;
            TimerDispBox.Text = formatTimeKeeper(time);
        }
        private String formatTimeKeeper(int t)
        {
         
            int m = t / 60;
            int s = t % 60;

            return String.Format("{0:d2}:{1:d2}", m, s);

        }

 
        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            if (formStatus == FormStatus.STOP)
            {
                if (timerStatus == TimerStaus.WORK)
                {
                    timerStart();
                }
                else if (timerStatus == TimerStaus.BREAK)
                {
                    timerRestart();
                }

            }
            else if (formStatus == FormStatus.START)
            {
                if (timerStatus == TimerStaus.WORK)
                {
                    timerStop();
                }
                else if (timerStatus == TimerStaus.BREAK)
                {
                    timerRestart();
                }
            }
    
        }


        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            if (formStatus == FormStatus.STOP)
            {
                if (timerStatus == TimerStaus.WORK)
                {
                    timerStatus = TimerStaus.BREAK;
                    timerStart();
                }
                else if (timerStatus == TimerStaus.BREAK)
                {
                    timerStart();
                }
            }
            else if (formStatus == FormStatus.START)
            {
                if (timerStatus == TimerStaus.WORK)
                {
                    timerSwitch();
                }
                else if (timerStatus == TimerStaus.BREAK)
                {

                    timerStop();
                }
            }
        }

        private void AnnonceText_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Properties.Resources.HOSTURL);

            }
            catch { }

           
        }

  

    }
}
