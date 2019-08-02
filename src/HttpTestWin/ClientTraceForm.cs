using System;
using System.Windows.Forms;
using Common;
using HttpTestWin.ViewModel;

namespace HttpTestWin
{
    public partial class ClientTraceForm : Form
    {
        public ClientTraceForm()
        {
            InitializeComponent();
            MyInitializeComponent();
        }

        public TestClientSpanApiVo Vo { get; set; }

        private void MyInitializeComponent()
        {
            this.cbxMethod.Items.Add("Post");
            this.cbxMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbxMethod.SelectedIndex = 0;
            this.txtResult.ScrollBars = ScrollBars.Vertical;
            this.txtData.ScrollBars = ScrollBars.Vertical;

            var processorCount = Environment.ProcessorCount;
            for (int i = 0; i < processorCount; i++)
            {
                this.cbxParallelCount.Items.Add(i + 1);
            }
            this.cbxParallelCount.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbxParallelCount.SelectedIndex = processorCount - 1;

            this.btnStart.Click += BtnStart_Click;

            Vo = SimpleIoc.Instance.Resolve<TestClientSpanApiVo>();
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            var config = TryReadHttpTestConfig();
            if (config == null)
            {
                MessageBox.Show(@"输入参数不合法");
                return;
            }

            var testClientSpans = Vo.CreateTestClientSpans(config);
            var testResults = await Vo.StartTest(config, testClientSpans);
            var summary = TestResultsSummary.Create(testResults.Items);

            this.txtData.Text = testClientSpans.ToJson(true);
            this.txtUri.Text = config.TraceApiEndPoint;

            testResults.Data = string.Empty;
            var resultsDesc = TestResultsHelper.CreateResultsDesc(testResults, summary);
            this.txtResult.Text = resultsDesc;
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
            var data = this.txtData.Text.Trim().Replace("\r\n", "");
            var httpTestConfig = HttpTestConfig.Instance;
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
