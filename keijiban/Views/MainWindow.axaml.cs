using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using keijiban.ViewModels;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace keijiban.Views
{
    public partial class MainWindow : Window
    {
        private CancellationTokenSource? _animationCts;
        private readonly object _animationLock = new object();

        public MainWindow()
        {
            InitializeComponent();

            // ウィンドウのライフサイクルイベントを購読
            Loaded += OnWindowLoaded;
            Closing += OnWindowClosing;
        }

        /// <summary>
        /// ウィンドウがロードされ、表示準備ができたときに呼び出されます。
        /// </summary>
        private async void OnWindowLoaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                // ViewModelのプロパティ変更イベントを購読
                vm.PropertyChanged += OnViewModelPropertyChanged;

                // 初期状態のアニメーションを適用
                ToggleMarqueeAnimation(vm.IsEmergencyActive);

                // ViewModelの非同期初期化を実行
                await vm.InitializeAsync();
            }
        }

        /// <summary>
        /// ウィンドウがクローズされる前に呼び出されます。
        /// </summary>
        private void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            // イベントの購読を解除し、メモリリークを防ぐ
            if (DataContext is MainViewModel vm)
            {
                vm.PropertyChanged -= OnViewModelPropertyChanged;
            }

            // 実行中のアニメーションをキャンセル
            _animationCts?.Cancel();
            _animationCts?.Dispose();
        }

        /// <summary>
        /// DataContext(ViewModel)のプロパティが変更されたときに呼び出されます。
        /// </summary>
        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not MainViewModel vm) return;

            // IsEmergencyActive プロパティが変更された場合（ON/OFFの切り替え）
            if (e.PropertyName == nameof(MainViewModel.IsEmergencyActive))
            {
                Dispatcher.UIThread.InvokeAsync(() => ToggleMarqueeAnimation(vm.IsEmergencyActive));
            }
            // または、EmergencyMarqueeText プロパティが変更され、かつ緊急情報がアクティブな場合
            // （表示するテキスト内容が更新された場合）
            else if (e.PropertyName == nameof(MainViewModel.EmergencyMarqueeText) && vm.IsEmergencyActive)
            {
                // アニメーションを再起動して、新しいテキストの長さに合わせる
                Dispatcher.UIThread.InvokeAsync(() => ToggleMarqueeAnimation(true));
            }
        }

        /// <summary>
        /// 緊急情報マーキーのアニメーションを開始または停止します。
        /// 単一のTextBlockを画面右端から左端へスクロールさせる方式です。
        /// </summary>
        private async void ToggleMarqueeAnimation(bool start)
        {
            var canvas = this.FindControl<Canvas>("MarqueeCanvas");
            var textBlock = this.FindControl<TextBlock>("MarqueeText");

            if (canvas is null || textBlock is null)
            {
                return;
            }

            // lockステートメントで、このメソッドへの同時アクセスを防ぐ
            lock (_animationLock)
            {
                // 既存のアニメーションがあればキャンセルし、リソースを解放
                _animationCts?.Cancel();
                _animationCts?.Dispose();
                _animationCts = null; // Dispose後にnullを代入して、破棄されたオブジェクトへのアクセスを防ぐ
            }

            // アニメーションを停止する際は、TextBlockの位置を初期位置（見えない場所）に戻す
            if (textBlock.RenderTransform is TranslateTransform initialTransform)
            {
                // アニメーション開始前に位置をリセットしておく
                initialTransform.X = canvas.Bounds.Width;
            }

            if (start)
            {
                _animationCts = new CancellationTokenSource();
                var newCts = new CancellationTokenSource();
                lock (_animationLock)
                {
                    // 新しいCancellationTokenSourceを安全にセット
                    _animationCts = newCts;
                }

                var cancellationToken = newCts.Token;

                try
                {
                    // UIのレイアウト計算が完了するのを待つための非同期処理
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        // サイズが確定するまで少し待つ
                        await Task.Delay(100, cancellationToken);
                        if (cancellationToken.IsCancellationRequested) return;

                        var canvasWidth = canvas.Bounds.Width;
                        var textWidth = textBlock.Bounds.Width;

                        // サイズが正しく取得できなければアニメーションは行わない
                        if (canvasWidth <= 0 || textWidth <= 0)
                        {
                            return;
                        }

                        // アニメーションの定義
                        var animation = new Animation
                        {
                            // 移動距離（Canvas幅＋TextBlock幅）を速度で割って継続時間を計算
                            Duration = TimeSpan.FromSeconds((canvasWidth + textWidth) / 150.0), // 150ピクセル/秒で移動
                            IterationCount = IterationCount.Infinite, // 無限に繰り返す
                            Children =
                    {
                        // 開始位置：画面の右端の外側 (X = Canvasの幅)
                        new KeyFrame
                        {
                            Cue = new Cue(0.0),
                            Setters = { new Setter(TranslateTransform.XProperty, canvasWidth) }
                        },
                        // 終了位置：画面の左端の外側 (X = -TextBlockの幅)
                        new KeyFrame
                        {
                            Cue = new Cue(1.0),
                            Setters = { new Setter(TranslateTransform.XProperty, -textWidth) }
                        }
                    }
                        };

                        // アニメーションを実行
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            await animation.RunAsync(textBlock, cancellationToken);
                        }

                    }, DispatcherPriority.Background, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    // キャンセルされた場合は何もしない
                }
                catch (Exception ex)
                {
                    // TaskCanceledException以外の予期せぬ例外を捕捉し、クラッシュを防ぐ
                    // ここにロギング処理を実装することを強く推奨します
                    Console.WriteLine($"[ERROR] Marquee animation failed: {ex.Message}");
                }
            }
        }
    }
}
