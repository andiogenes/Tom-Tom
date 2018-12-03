using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TomTom
{
    class TomTomContext : ApplicationContext
    {
        // ------------- Hardcoded parameters ----------------- //
        private const int WORK_TIME = 25 * 60;
        private const int REST_TIME = 5 * 60;
        private const int EXTRA_REST_TIME = 20 * 60;
        private const int EXTRA_REST_SESSION_INTERVAL = 4;

        private enum Activities { WORK, REST, EXTRA_REST };

        private const string WORK_MESSAGE = "За работу! 🔥";
        private const string REST_MESSAGE = "Пять минут отдыха! ☕";
        private const string EXTRA_REST_MESSAGE = "Большой перерыв - 20 минут! 💤";

        // ----------- Timers ------------ //
        private Timer timer = new Timer();


        // ------- Notification icons ------ //
        NotifyIcon notify;

        // ----------- Menu Items ---------- //
        private MenuItem infoItem = new MenuItem();

        // --------------------- State ------------------- //
        private int timeUntilActivitySwap = WORK_TIME;
        private int currentSessionNo = 1;
        private Activities currentActivity = Activities.WORK;

        public TomTomContext()
        {
            notify = new NotifyIcon
            {
                Icon = Properties.Resources.pomodoro,
                BalloonTipText = WORK_MESSAGE,
                BalloonTipTitle = "Tom-Tom"
            };

            MenuItem pauseItem = new MenuItem("Pause",
                (o, e) =>
                {
                    MenuItem self = (MenuItem)o;

                    self.Text = timer.Enabled ? "Continue" : "Pause";
                    timer.Enabled = !timer.Enabled;
                });

            MenuItem exitItem = new MenuItem("Exit", 
                (s, e) => 
                {
                    notify.Visible = false;
                    Application.Exit();
                });

            notify.ContextMenu = new ContextMenu(new MenuItem[] { infoItem, pauseItem, exitItem });
            notify.Visible = true;

            timer.Interval = 1000;
            timer.Tick += TimerTicker;
            timer.Start();

            notify.ShowBalloonTip(2000);
        }

        private void TimerTicker(Object o, EventArgs e)
        {
            infoItem.Text = string.Format("{0}: {01}:{02}",
                currentActivity == Activities.WORK ? "Work" : "Rest",
                //currentActivity == Activities.WORK ? "🔥🔨" : "☕💤",
                timeUntilActivitySwap / 60,
                timeUntilActivitySwap % 60);

            if (--timeUntilActivitySwap <= 0)
            {
                // Latest activity was working session => 
                // => change to (extra) rest and (reset)/increase number of session.
                if (currentActivity == Activities.WORK)
                {
                    System.Media.SystemSounds.Asterisk.Play();

                    if (++currentSessionNo > EXTRA_REST_SESSION_INTERVAL)
                    {
                        currentActivity = Activities.EXTRA_REST;
                        currentSessionNo = 1;
                        timeUntilActivitySwap = EXTRA_REST_TIME;

                        notify.BalloonTipText = EXTRA_REST_MESSAGE;
                    }
                    else
                    {
                        currentActivity = Activities.REST;
                        timeUntilActivitySwap = REST_TIME;

                        notify.BalloonTipText = REST_MESSAGE;
                    }
                
                }
                else
                {
                    System.Media.SystemSounds.Hand.Play();

                    currentActivity = Activities.WORK;
                    timeUntilActivitySwap = WORK_TIME;

                    notify.BalloonTipText = WORK_MESSAGE;

                }
                notify.ShowBalloonTip(2000);
            }
        }
    }

    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.Run(new TomTomContext());
        }
    }
}
