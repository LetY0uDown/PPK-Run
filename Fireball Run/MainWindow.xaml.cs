using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.IO;
using System.Xml.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireball_Run
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            timer.Interval = new TimeSpan(10000);
            jumpTimer.Interval = TimeSpan.FromSeconds(1.2);

            jumpTimer.Tick += JumpTimer_Tick;
            timer.Tick += Timer_Tick;
        }

        private const int FIREBALL_START_POS = 810;
        private const int GAME_FIELD_HEIGHT = 100;

        private const int FIREBALL_WIDTH = 60;
        private const int FIREBALL_HEIGHT = 50;

        private const int CHARACTER_POS = 50;

        private int fireballSpeed
        {
            get
            {
                if (2 + scores / 1500 >= 5)
                    return 5;
                else
                    return 2 + scores / 1500;
            }
        }

        private int scores = 0;
        private int personalBest = 0;

        private List<Entity> fireballs = new();

        private DispatcherTimer timer = new();
        private DispatcherTimer jumpTimer = new();

        private readonly Random random = new();

        private readonly Entity character = new()
        {
            Body = new Image
            {
                Source = new BitmapImage(new Uri("Images/Max.png", UriKind.Relative)),
                Width = 70,
                Height = 90
            },
            Position = new()
            {
                X = CHARACTER_POS,
                Y = GAME_FIELD_HEIGHT
            }
        };

        private void StartNewGame()
        {
            GameField.Children.Add(new Image
            {
                Source = new BitmapImage(new Uri("Images/Anton.png", UriKind.Relative)),
                Height = 150
            });

            Canvas.SetBottom(GameField.Children[0], GAME_FIELD_HEIGHT);
            Canvas.SetRight(GameField.Children[0], 20);

            scores = 0;
            fireballs.Clear();
            GameField.Children.Add(character.Body);

            Canvas.SetBottom(character.Body, GAME_FIELD_HEIGHT);
            Canvas.SetLeft(character.Body, CHARACTER_POS);

            tbNewRecord.Visibility = Visibility.Collapsed;

            timer.Start();
        }
        private void StopGame()
        {
            GameField.Children.Clear();

            tbTitle.Text = "Ты отчислен!";
            tbTitle.Foreground = Brushes.Red;

            if (scores > personalBest)
            {
                SaveRecord();
                tbNewRecord.Visibility = Visibility.Visible;
            }

            MenuPanel.Visibility = Visibility.Visible;

            timer.Stop();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (fireballs.Count == 0 || CanEnemySpawn())
                SpawnEnemy();

            foreach (var enemy in fireballs.ToList())
            {
                MoveEnemy(enemy);
                CollisionCheck(enemy);
            }

            tbScores.Text = $"Score: {scores++}";

            if (scores > personalBest && scores < personalBest + 500)
                tbNewRecord.Visibility = Visibility.Visible;
            else
                tbNewRecord.Visibility = Visibility.Collapsed;
        }

        private void CollisionCheck(Entity enemy)
        {
            if (enemy.Position.X < character.Position.X + CHARACTER_POS && enemy.Position.X + 60 > character.Position.X
                && enemy.Position.Y == character.Position.Y)
            {
                StopGame();
            }
        }

        private void MoveEnemy(Entity enemy)
        {
            enemy.Position.X -= fireballSpeed;
            Canvas.SetLeft(enemy.Body, enemy.Position.X);

            if (enemy.Position.X < -20)
                RemoveEnemy(enemy);
        }

        private bool CanEnemySpawn()
        {
            return Canvas.GetLeft(fireballs[^1].Body) < FIREBALL_START_POS - 210 && random.Next(0, 300) == 0;
        }
        private void SpawnEnemy()
        {
            fireballs.Add(new Entity
            {
                Body = new Image
                {
                    Source = new BitmapImage(new Uri("Images/Knijechka.png", UriKind.Relative)),
                    Width = FIREBALL_WIDTH,
                    Height = FIREBALL_HEIGHT
                },
                Position = new()
                {
                    X = FIREBALL_START_POS,
                    Y = GAME_FIELD_HEIGHT
                }
            });

            GameField.Children.Add(fireballs[^1].Body);

            Canvas.SetBottom(fireballs[^1].Body, GAME_FIELD_HEIGHT);
            Canvas.SetLeft(fireballs[^1].Body, FIREBALL_START_POS);

        }
        private void RemoveEnemy(Entity enemy)
        {
            fireballs.Remove(enemy);
            GameField.Children.Remove(enemy.Body);
        }

        private void SetCharacterHeight(int newHeight)
        {
            character.Position.Y = newHeight;
            Canvas.SetBottom(character.Body, newHeight);
        }

        private void Jump()
        {
            jumpTimer.Start();
            character.Position.Y += 70;
            Canvas.SetBottom(character.Body, character.Position.Y);
        }

        private void JumpTimer_Tick(object? sender, EventArgs e)
        {
            character.Position.Y = GAME_FIELD_HEIGHT;
            Canvas.SetBottom(character.Body, GAME_FIELD_HEIGHT);

            jumpTimer.Stop();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && character.Position.Y == GAME_FIELD_HEIGHT)
                Jump();
        }

        private void StartImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GameField.Children.Clear();
            StartNewGame();
            MenuPanel.Visibility = Visibility.Collapsed;
        }

        private void ExitImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LoadRecord()
        {
            if (File.Exists("personalBest.xml"))
            {
                XmlSerializer? serializer = new(typeof(int));

                using (Stream reader = new FileStream("personalBest.xml", FileMode.Open))
                {
                    personalBest = (int)serializer.Deserialize(reader);
                }

                tbPersonalBest.Text = $"Personal Best: {personalBest}";
            }
        }

        private async void SaveRecord()
        {
            XmlSerializer serializer = new(typeof(int));

            personalBest = scores;

            using (Stream writer = new FileStream("personalBest.xml", FileMode.Create))
            {
                await Task.Run(() => serializer.Serialize(writer, personalBest));
            }

            tbPersonalBest.Text = $"Personal Best: {personalBest}";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRecord();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
