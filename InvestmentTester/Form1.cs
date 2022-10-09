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

        float buyOrSellFee = 0.006f;
        //float coinPrice = 1234.56789f;
        float coinPrice = 0.0f;
        float startAmount;
        float checking;
        float checking_;
        float currBought;
        float totalYeild;
        float currYeild;
        float changeToday;
        float maxPercChangePerDay;
        float maxPercBuyOrSell;
        float sumPercChange;
        float[] btcPrice;

        int days;
        int dayDuration;
        int safetyHalving;
        int currStage;
        int numSales;

        float[] purchaseArray;

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] floats = File.ReadAllLines("price.txt");
            btcPrice = new float[floats.Length];
            for (int i = 0; i < btcPrice.Length; i++) btcPrice[i] = float.Parse(floats[i]);


            days = 0;
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
            purchaseArray = new float[safetyHalving];
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
            purchaseArray = new float[safetyHalving];
            label6.Text = "starts: $" + (Math.Pow(0.5, safetyHalving) * startAmount);
        }

        private void textBox4_TextChanged(object sender, EventArgs e) //Max % Change a Day
        {
            maxPercChangePerDay = Float(textBox4.Text);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //changeToday = (maxPercChangePerDay * (float)ran.NextDouble());
            changeToday = ((btcPrice[days + 1] / btcPrice[days]) - 1) * 100.0f;

            bool increase = btcPrice[days + 1] > btcPrice[days];

            if (increase) //Increase or Decrease
            {
                //float mul = (1 + (changeToday / 100));
                float mul = (1 + (changeToday / 100));
                coinPrice = btcPrice[days];
                //coinPrice *= mul;
                UpdatePurchaseArray(mul);
                sumPercChange += changeToday;
            }
            else
            {
                //float mul = (1 - (changeToday / 100));
                float mul = (1 + (changeToday / 100));
                coinPrice = btcPrice[days];
                //coinPrice *= mul;
                UpdatePurchaseArray(mul);
                sumPercChange += changeToday;
            }

            if (sumPercChange > maxPercBuyOrSell) //sell
            {
                Sell();

                currStage = 1;
                Buy((float)(Math.Pow(0.5, safetyHalving) * checking)); //Just automatically buy again, don't wait
                sumPercChange = 0.0f;

            }
            else if (sumPercChange < -maxPercBuyOrSell) //Keep Investing
            {
                if (currStage + 1 > safetyHalving)
                {
                    richTextBox1.Text += "\nUh oh... going to have to do some waiting";
                }
                else
                {
                    currStage++;
                    Buy((float)(Math.Pow(0.5, safetyHalving - (currStage - 1)) * checking));
                }
            }
            totalYeild = checking_ - startAmount;
            days++;
            UpdateValues();
        }

        void Buy(float amount)
        {
            amount *= 1.0f - buyOrSellFee;
            richTextBox1.Text += "\n -" + maxPercBuyOrSell + "% hit! Buying " + amount;
            checking -= amount;
            currBought = checking_ - checking;

            purchaseArray[currStage - 1] = amount;

            sumPercChange = 0.0f;
        }
        void Sell()
        {
            checking += PurchaseArraySum() * (1.0f - buyOrSellFee);

            currYeild = checking - checking_;

            checking_ = checking;
            richTextBox1.Text += "\nBuy no More!";
            currBought = 0;

            richTextBox2.Text += "\n+ $" + currYeild + " made";
            chart1.Series["Yeild"].Points.AddXY(numSales, currYeild);
            numSales++;
        }
        float PurchaseArraySum()
        {
            float ret = 0.0f;
            for (int i = 0; i < currStage; i++) ret += purchaseArray[i];
            return ret;
        }
        void UpdatePurchaseArray(float mul)
        {
            for (int i = 0; i < currStage; i++)
            {
                purchaseArray[i] *= mul;
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
            label17.Text = "Total Yeild: $" + $"{totalYeild:0.00}";
            label18.Text = "Sales: " + numSales;
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
    }
}
