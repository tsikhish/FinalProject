using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Channels;
using System.Transactions;

namespace FinalProj
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filePath = @"C:\Users\Mari\source\repos\pracForFinaleProj\pracForFinaleProj\file.json";
            var mycardInformations = LoadJsonFile(filePath);
            while (true)
            {
               bool isvalid = isValid(mycardInformations.cardDetails.cardNumber,
                                      mycardInformations.cardDetails.CVC,
                                      mycardInformations.cardDetails.expirationDate);
                if (isvalid)
                {
                    Console.WriteLine("Please enter your pin: ");
                    string pincode = Console.ReadLine();
                    if (pincode == mycardInformations.PinCode)
                    {
                        transactions(filePath);
                    }
                    else
                    {
                        Console.WriteLine("your pincode is invalid");
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid card information. Please try again.");
                }
            }
        }
        public static JasonClass LoadJsonFile(string filePath)
        {
            try
            {
                using (StreamReader r = new StreamReader(filePath))
                {
                    var jsonString = r.ReadToEnd();
                    return JsonConvert.DeserializeObject<JasonClass>(jsonString);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading JSON file: {ex.Message}");
                return null;
            }
        }
        public static bool isValid(string cardNumber, string CVC, string expirationDate)
        {
            Console.WriteLine("please enter card number: ");
            string cardnum = Console.ReadLine();
            Console.WriteLine("Please enter CVC: ");
            string cvc = Console.ReadLine();
            Console.WriteLine("Please enter expirationDate: ");
            string expirationdate = Console.ReadLine();
            bool isValid = false;
                if (cardNumber == cardnum && CVC == cvc && expirationDate == expirationdate)
                {
                    isValid = true;
                }
                else
                {
                    isValid = false;
                }
            
            return isValid;
        }
        
        public static void transactions(string filePath)
        {
            JasonClass transactions = LoadJsonFile(filePath);
            List<transactionHistory> similarTotransHist = new List<transactionHistory>();
            Console.WriteLine("choose number of desired action: ");
            Console.WriteLine("1.Check Deposit ");
            Console.WriteLine("2.Get Amount");
            Console.WriteLine("3.Get Last 5 Transactions ");
            Console.WriteLine("4.Add Amount");
            Console.WriteLine("5.Change PIN ");
            Console.WriteLine("6.Change Amount");
            int action = int.Parse(Console.ReadLine());
           
            switch (action)
            {
                case 1:
                    checkDeposit(transactions, similarTotransHist);
                    break;
                case 2:
                    getAmount(transactions, similarTotransHist);
                    break;
                case 3:
                    Last5(transactions, similarTotransHist);
                    break;
                case 4:
                    fillAmount(transactions, similarTotransHist);
                    break;
                case 5:
                    changePinCode(transactions, similarTotransHist);
                    break;
                case 6:
                    changeCurrency(transactions, similarTotransHist);
                    break;
                default:
                    Console.WriteLine("this action doesnt exists");
                    break;
            }
          transactions.transactionHistory.AddRange(similarTotransHist);
          
            try
            {
                string updatedJsonContent = JsonConvert.SerializeObject(transactions, Formatting.Indented);
                File.WriteAllText(filePath, updatedJsonContent);
                Console.WriteLine("JSON file updated successfully.");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
          
        }
        public static void checkDeposit(JasonClass transactions, List<transactionHistory> lst)
        {
            foreach (var transaction in transactions.transactionHistory.Where(transaction => transaction.transactionType == "DepositCheck"))
            {
                Console.WriteLine("Amount is: ");
                Console.WriteLine($"GEL: {transaction.amountGEL}");
                Console.WriteLine($"EUR: {transaction.amountEUR}");
                Console.WriteLine($"USD: {transaction.amountUSD}");
                transaction.transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");

                lst.Add( transaction );
            }
        }
        public static void getAmount(JasonClass transactions, List<transactionHistory> lst)
        {
            foreach (var transaction in transactions.transactionHistory.Where(transaction => transaction.transactionType == "GetAmount"))
            {
                Console.WriteLine("Please enter how much money you want to get: ");
                int money = int.Parse(Console.ReadLine());
                if (money > transaction.amountGEL) { Console.WriteLine("Invalid."); return; }
                else
                {
                    transaction.amountGEL = transaction.amountGEL - money;
                    transaction.transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
                    lst.Add(transaction);

                }
            }
        }
        public static void Last5(JasonClass transactions, List<transactionHistory> lst)
        {
            Console.WriteLine("Last 5 Transactions:");
            var lastFiveTransactions = transactions.transactionHistory.TakeLast(5);
            foreach (var transaction in lastFiveTransactions)
            {
                Console.WriteLine($"Date: {transaction.transactionDate}, Type: {transaction.transactionType}, Amount: {transaction.amountGEL}");
            }
        }

        public static void fillAmount(JasonClass transactions, List<transactionHistory> lst)
        {
            foreach (var transaction in transactions.transactionHistory.Where(transaction => transaction.transactionType == "FillAmount"))
            {
                Console.WriteLine("Please fill the bank account ");
                int money = int.Parse(Console.ReadLine());
                transaction.amountGEL = transaction.amountGEL + money;
                transaction.transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
                lst.Add(transaction);
            }
        }
        public static void changePinCode(JasonClass transactions, List<transactionHistory> lst)
        {
            Console.WriteLine("Please enter new PIN: ");
            string pincode = Console.ReadLine();
            transactions.PinCode = pincode;
            lst.RemoveAll(transaction => transaction.transactionType == "ChangePin");

            transactionHistory pinChangeTransaction = new transactionHistory
            {
                transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                transactionType = "ChangePin",
                amountGEL = 0,
                amountUSD = 0,
                amountEUR = 0
            };
            lst.Add(pinChangeTransaction);
        }

        public static void changeCurrency(JasonClass transactions, List<transactionHistory> lst)
        {
            foreach (var transaction in transactions.transactionHistory.Where(transaction => transaction.transactionType == "Change currency"))
            {
                Console.WriteLine("Please enter in which currency you want to change your balance: EUR OR USD");
                string currency = Console.ReadLine();
                if (currency == "USD")
                {
                    transaction.amountUSD = transaction.amountGEL * 0.37;
                    transaction.amountGEL = 0;
                }
                else if (currency == "EUR")
                {
                    transaction.amountEUR = transaction.amountGEL * 0.34;
                    transaction.amountGEL = 0;

                }
                transaction.transactionDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");

                lst.Add(transaction);

            }
        }
        




    }
}



