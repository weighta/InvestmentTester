using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace InvestmentTester
{
    public partial class Form1 : Form
    {
        Random ran = new Random();
        public Form1()
        {
            InitializeComponent();
        }

        double coinPrice = 1234.56789;
        //double coinPrice = 0.0f;
        double buyOrSellFee = 0.006;
        double startAmount;
        double checking;
        double checking_;
        double currBought;
        double totalYeild;
        double currYeild;
        double changeToday;
        double maxPercChangePerDay;
        double maxPercBuyOrSell;
        double sumPercChange;
        double[] btcPrice;

        int days;
        int highestStage;
        int dayDuration;
        int safetyHalving;
        int currStage;
        int numSales;
        int numDangers;

        double[] prepurchaseArray;
        double[] purchaseArray;
        double[] coinPriceArray;

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] floats = File.ReadAllLines("price.txt");
            btcPrice = new double[floats.Length];
            for (int i = 0; i < btcPrice.Length; i++) btcPrice[i] = float.Parse(floats[i]);


            days = 0;
            numDangers = 0;
            startAmount = Int(textBox1.Text);
            checking = startAmount;
            checking_ = checking;
            currBought = 0.0f;
            totalYeild = 0.0f;
            currYeild = 0.0f;
            changeToday = 0.0f;
            numSales = 0;
            dayDuration = Int(textBox2.Text) * 1000;
            safetyHalving = Int(textBox3.Text);
            maxPercChangePerDay = Float(textBox4.Text);
            maxPercBuyOrSell = Float(textBox5.Text);
            sumPercChange = 0.0f;
            currStage = 1;
            prepurchaseArray = new double[safetyHalving];
            purchaseArray = new double[safetyHalving];
            coinPriceArray = new double[safetyHalving];
            timer1.Interval = dayDuration;
        }

        int Int(string a)
        {
            try
            {
                return Int32.Parse(a);
            }
            catch { return 0; }
        }
        float Float(string a)
        {
            try
            {
                return float.Parse(a);
            }
            catch { return 0.0f; }
        }

        private void button1_Click(object sender, EventArgs e) //Begin Simulation
        {
            Buy((float)(Math.Pow(0.5, safetyHalving) * checking)); //Just automatically buy again, don't wait
            timer1.Start();
        }

        private void textBox2_TextChanged(object sender, EventArgs e) //Day Duration
        {
            dayDuration = (int)(Float(textBox2.Text) * 1000.0f);
            if(dayDuration <= 0)
            {
                timer1.Interval = 1000;
            }
            else
            {
                timer1.Interval = dayDuration;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e) //Safety Halves
        {
            safetyHalving = Int(textBox3.Text);
            purchaseArray = new double[safetyHalving];
            coinPriceArray = new double[safetyHalving];
            prepurchaseArray = new double[safetyHalving];
            label6.Text = "starts: $" + (Math.Pow(0.5, safetyHalving) * startAmount);
        }

        private void textBox4_TextChanged(object sender, EventArgs e) //Max % Change a Day
        {
            maxPercChangePerDay = Float(textBox4.Text);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (checkBox1.Checked) //Random Data
            {
                changeToday = (maxPercChangePerDay * (float)ran.NextDouble());
                bool increase = Convert.ToBoolean(ran.Next(0, 2));
                if (increase) //Increase or Decrease
                {
                    double mul = (1 + (changeToday / 100));
                    coinPrice *= mul;
                    UpdatePurchaseArray();
                    sumPercChange += changeToday;
                }
                else
                {
                    double mul = (1 - (changeToday / 100));
                    coinPrice *= mul;
                    UpdatePurchaseArray();
                    sumPercChange -= changeToday;
                    changeToday *= -1;
                }
            }
            else //Use BTC Price
            {
                changeToday = ((btcPrice[days + 1] / btcPrice[days]) - 1) * 100.0f;
                bool increase = btcPrice[days + 1] > btcPrice[days];
                if (increase) //Increase or Decrease
                {
                    coinPrice = btcPrice[days];
                    UpdatePurchaseArray();
                    sumPercChange += changeToday;
                }
                else
                {
                    coinPrice = btcPrice[days];
                    UpdatePurchaseArray();
                    sumPercChange += changeToday;
                }
            }
            

            if (sumPercChange > maxPercBuyOrSell) //sell
            {
                Sell();

                currStage = 1;
                Buy((float)(Math.Pow(0.5, safetyHalving) * checking)); //Just automatically buy again, don't wait
                sumPercChange -= maxPercBuyOrSell;
            }
            else if (currStage >= safetyHalving)
            {
                if (sumPercChange < (-maxPercBuyOrSell / 10.0))
                {
                    Sell();

                    currStage = 1;
                    Buy((float)(Math.Pow(0.5, safetyHalving) * checking)); //Just automatically buy again, don't wait
                    sumPercChange += maxPercBuyOrSell;
                }
            }
            else if (sumPercChange < -maxPercBuyOrSell) //Keep Investing
            {
                if (currStage >= safetyHalving)
                {
                    label12.Text = "Status: Unstable";
                }
                else
                {
                    label12.Text = "Status: Stable";
                    currStage++;
                    Buy(Math.Pow(0.5, safetyHalving - (currStage - 1)) * checking);
                    sumPercChange += maxPercBuyOrSell;
                }
            }
            totalYeild = checking_ - startAmount;
            days++;
            UpdateValues();
        }

        void Buy(double amount)
        {
            amount *= 1.0f - buyOrSellFee;
            //richTextBox1.Text += "\n -" + maxPercBuyOrSell + "% hit! Buying " + amount;
            checking -= amount;
            currBought = checking_ - checking;

            prepurchaseArray[currStage - 1] = amount;
            coinPriceArray[currStage - 1] = coinPrice;
            if (currStage > highestStage) highestStage = currStage;
        }
        void Sell()
        {
            checking += PurchaseArraySum() * (1.0f - buyOrSellFee);

            currYeild = checking - checking_;

            checking_ = checking;
            //richTextBox1.Text += "\nBuy no More!";
            currBought = 0;

            label14.Text = "Recent Yeild: +" + $"{currYeild:0.00}";
            chart1.Series["Yeild"].Points.AddXY(numSales, currYeild);
            if (safetyHalving >> 1 <= currStage)
            {
                chart3.Series["Danger Zone"].Points.AddXY(numDangers++, currStage);
                chart4.Series["Large Income"].Points.AddXY(numDangers, currYeild);
            }
            
            numSales++;
        }
        double PurchaseArraySum()
        {
            double ret = 0.0f;
            for (int i = 0; i < currStage; i++) ret += purchaseArray[i];
            return ret;
        }
        void UpdatePurchaseArray()
        {
            for (int i = 0; i < currStage; i++)
            {
                purchaseArray[i] = prepurchaseArray[i] * (coinPrice / coinPriceArray[i]);
            }

            string a = "";
            for (int i = 0; i < currStage; i++)
            {
                a += " " + $"{purchaseArray[i]:0.00}";
            }
            label16.Text = "Purchase Array: " + a;
        }
        void UpdateValues()
        {
            label8.Text = "Checking: " + checking;
            label15.Text = "Curr Stage: " + currStage;
            label11.Text = "Curr Bought: $" + currBought;
            label7.Text = "Change Today: " + changeToday + "%";
            label13.Text = "Sum Change: " + sumPercChange + "%";
            label3.Text = "Coin Price: " + coinPrice;
            label2.Text = "Day: " + days;
            label17.Text = "Total Yeild: $"+totalYeild;
            label18.Text = "Sales: " + numSales;
            label19.Text = "Highest Stage: " + highestStage;
            chart2.Series["Coin Price"].Points.AddXY(days, coinPrice);
        }

        private void textBox1_TextChanged(object sender, EventArgs e) //Start Amount
        {
            startAmount = Int(textBox1.Text);
            checking = startAmount;
            checking_ = checking;
        }

        private void textBox5_TextChanged(object sender, EventArgs e) //Max % Buy Or Sell
        {
            maxPercBuyOrSell = Float(textBox5.Text);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                coinPrice = 1234.56789f;
            }
            else
            {
                coinPrice = 0.0f;
            }
        }
    }
}
