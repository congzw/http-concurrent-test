using System;
using System.Threading;
using System.Windows.Forms;
using Common;
using HttpTestWin.ViewModel;

namespace HttpTestWin
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            MyInitializeComponent();
        }

        public MainVo MainVo { get; set; }

        private void MyInitializeComponent()
        {
            this.Closing += MainForm_Closing;
            this.cbxMethod.Items.Add("Get");
            this.cbxMethod.Items.Add("Post");
            this.cbxMethod.Items.Add("Put");
            this.cbxMethod.Items.Add("Delete");
            this.cbxMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            this.txtResult.ScrollBars = ScrollBars.Vertical;

            var processorCount = Environment.ProcessorCount;
            for (int i = 0; i < processorCount; i++)
            {
                this.cbxParallelCount.Items.Add(i + 1);
            }
            this.cbxParallelCount.DropDownStyle = ComboBoxStyle.DropDownList;

            MainVo = SimpleIoc.Instance.Resolve<MainVo>();
            var httpTestConfig = MainVo.LoadConfig();
            SetValueWithConfig(httpTestConfig);
        }


        private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var config = TryReadHttpTestConfig();
            if (config == null)
            {
                return;
            }
            MainVo.SaveConfig(config);
        }

        private void MainForm_Load(object sender, System.EventArgs e)
        {
        }

        private async void btnStart_Click(object sender, System.EventArgs e)
        {
            var config = TryReadHttpTestConfig();
            if (config == null)
            {
                MessageBox.Show(@"输入参数不合法");
                return;
            }

            var testResults = await MainVo.StartTest(config);
            var summary = TestResultsSummary.Create(testResults.Items);
            var resultsDesc = MainVo.CreateResultsDesc(testResults, summary);
            this.txtResult.Text = resultsDesc;
        }

        private void SetValueWithConfig(HttpTestConfig httpTestConfig)
        {
            this.txtConcurrentCount.Text = httpTestConfig.ConcurrentCount.ToString();
            this.cbxParallelCount.SelectedItem = httpTestConfig.MaxParallelCount;
            this.txtExpired.Text = httpTestConfig.FailExpiredMs.ToString();
            this.cbxMethod.SelectedItem = httpTestConfig.HttpMethod;
            this.txtUri.Text = httpTestConfig.LastTestUri;
            this.txtData.Text = httpTestConfig.LastTestData;
        }

        private HttpTestConfig TryReadHttpTestConfig()
        {
            if (!int.TryParse(this.txtConcurrentCount.Text.Trim(), out int concurrent))
            {
                return null;
            }

            if (!int.TryParse(this.txtExpired.Text.Trim(), out int expiredMs))
            {
                return null;
            }


            var method = this.cbxMethod.SelectedItem.ToString();
            var uri = this.txtUri.Text.Trim();
            var data = this.txtData.Text.Trim();
            var httpTestConfig = new HttpTestConfig();
            httpTestConfig.MaxParallelCount = int.Parse(this.cbxParallelCount.SelectedItem.ToString());
            httpTestConfig.ConcurrentCount = concurrent;
            httpTestConfig.FailExpiredMs = expiredMs;
            httpTestConfig.HttpMethod = method;
            httpTestConfig.LastTestUri = uri;
            httpTestConfig.LastTestData = data;
            return httpTestConfig;
        }
    }
}
