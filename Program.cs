using System;
using System.Collections.Generic;
using System.Linq;

namespace L50_carService
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CarService carService = new CarService();

            carService.Run();
        }
    }

    class CarService
    {
        private List<int> _priceListDetails = new List<int>();
        private List<int> _priceListWorks = new List<int>();
        private List<int> _detailsCount = new List<int>();

        private List<Detail> _storage = new List<Detail>();
        private List<string> _detailsList;
        private Car _currentCar;
        private float _penaltyPercentage;
        private int _money;

        public CarService()
        {
            _detailsList = Enum.GetNames(typeof(DetailType)).ToList();
            _penaltyPercentage = 0.15f;
            _money = 150;

            FillStorage();
            FillPriceLists();
        }

        private enum Menu
        {
            FindPartInStock,
            RefuseClient,
            RepairCar,
            Exit,
        }

        public IReadOnlyList<string> DetailList => _detailsList;

        public void Run()
        {
            _currentCar = GetNewClient();
            bool isOpen = true;

            while (isOpen)
            {
                Console.Clear();

                if (_money > 0)
                {
                    int partIndex = GetBrokenDetailIndex();
                    ShowMenu(partIndex);
                    Console.Write("Выберите действие: ");

                    if (int.TryParse(Console.ReadLine(), out int number))
                    {
                        number--;

                        switch ((Menu)number)
                        {
                            case Menu.FindPartInStock:
                                FindDetailOnStorage((DetailType)partIndex);
                                break;

                            case Menu.RefuseClient:
                                RefuseClient(partIndex);
                                break;

                            case Menu.RepairCar:
                                RepairCar(partIndex);
                                break;

                            case Menu.Exit:
                                isOpen = false;
                                continue;

                            default:
                                ShowError();
                                break;
                        }
                    }
                    else
                    {
                        ShowError();
                    }
                }
                else
                {
                    isOpen = false;
                    Console.WriteLine("У вас недостаточно наличных. Вы банкрот!");
                }

                Console.ReadKey(true);
            }
        }

        private void RepairCar(int indexBrokenDetail)
        {
            Console.Clear();

            for (int i = 0; i < _detailsList.Count; i++)
                Console.WriteLine($"{i + 1} - {_detailsList[i]}");

            Console.Write("\nВыберите номер детали для замены: ");

            if (int.TryParse(Console.ReadLine(), out int numberDetail))
            {
                numberDetail--;

                if (TryGetDetail(out Detail newDetail, (DetailType)numberDetail))
                {
                    if (indexBrokenDetail == numberDetail)
                    {
                        _currentCar.ReplaceDetail(newDetail);
                        _money += _priceListDetails[numberDetail];
                        _money += _priceListWorks[numberDetail];
                        Console.WriteLine("Деталь успешна заменена.");
                    }
                    else
                    {
                        Console.Write("Вы заменили не ту деталь");
                        PayFine(numberDetail);
                    }
                }
                else
                {
                    Console.Write($"На складе нет выбранной детали");
                    PayFine(numberDetail);
                }

                _currentCar = GetNewClient();
            }
            else
            {
                ShowError();
            }
        }

        private void PayFine(int detailIndex)
        {
            int penalty = _priceListDetails[detailIndex] + _priceListWorks[detailIndex];
            _money -= penalty;
            Console.WriteLine($" и вынуждены возместить ушерб в размере: {penalty}.");
        }

        private void RefuseClient(int partIndex)
        {
            int penalty = (int)(_priceListDetails[partIndex] * _penaltyPercentage);
            _money -= penalty;
            Console.WriteLine($"Вы отказали клиенту и вынуждены были оплатить штраф: {penalty}");

            _currentCar = GetNewClient();
        }

        private void ShowMenu(int partIndex)
        {
            const char DelimeterSymbol = '=';
            const int DelimeterLenght = 50;

            string delimeter = new string(DelimeterSymbol, DelimeterLenght);

            if (partIndex >= 0)
            {
                Console.WriteLine($"У клиента сломан: {_detailsList[partIndex]}.\tЦена детали: {_priceListDetails[partIndex]}" +
                                  $".\tЦена за замену: {_priceListWorks[partIndex]}.\nВаш наличный баланс: {_money}.");
            }
            else
            {
                Console.WriteLine("В машине клинте поломка не обнаружена.");
            }

            Console.WriteLine(delimeter);
            Console.WriteLine($"{((int)Menu.FindPartInStock) + 1} - Найти деталь на складе.\n{((int)Menu.RefuseClient) + 1} - Отказать клиенту.\n" +
                              $"{((int)Menu.RepairCar) + 1} - Починить машину клиента.\n{((int)Menu.Exit) + 1} - Выйти.");
            Console.WriteLine(delimeter);
        }

        private int GetBrokenDetailIndex()
        {
            if (_currentCar.TryFindBrokenDetail(out Detail brokenDetail))
                for (int i = 0; i < _detailsList.Count; i++)
                    if (_detailsList[i].ToLower() == brokenDetail.Type.ToString().ToLower())
                        return i;

            return -1;
        }

        private void FindDetailOnStorage(DetailType type)
        {
            bool isAvailable = false;

            foreach (var detail in _storage)
                if (detail.Type == type)
                    isAvailable = true;

            if (isAvailable)
                Console.WriteLine("Деталь есть в наличии.");
            else
                Console.WriteLine("Такой детали нет на складе.");
        }

        private bool TryGetDetail(out Detail detail, DetailType detailType)
        {
            for (int i = 0; i < _storage.Count; i++)
            {
                if (_storage[i].Type == detailType)
                {
                    _detailsCount[i]--;

                    if (_detailsCount[i] == 0)
                    {
                        _detailsCount.RemoveAt(i);
                        _storage.RemoveAt(i);
                    }

                    detail = new Detail(detailType, true);
                    return true;
                }
            }

            detail = null;
            return false;
        }

        private Car GetNewClient() => new Car(DetailList);

        private void FillPriceLists()
        {
            const int MaxPriceDetail = 200;
            const int MinPriceDetail = 55;
            const int MaxPriceWork = 100;
            const int MinPriceWork = 20;

            for (int j = 0; j < _detailsList.Count; j++)
            {
                _priceListDetails.Add(RandomGenerator.GetRandomNumber(MinPriceDetail, MaxPriceDetail + 1));
                _priceListWorks.Add(RandomGenerator.GetRandomNumber(MinPriceWork, MaxPriceWork + 1));
            }
        }

        private void FillStorage()
        {
            const int MaxDetailsCount = 12;
            const int MinDetailsCount = 4;

            _storage.Add(new Detail(DetailType.Engine, true));
            _detailsCount.Add(RandomGenerator.GetRandomNumber(MinDetailsCount, MaxDetailsCount + 1));
            _storage.Add(new Detail(DetailType.Carburetor, true));
            _detailsCount.Add(RandomGenerator.GetRandomNumber(MinDetailsCount, MaxDetailsCount + 1));
            _storage.Add(new Detail(DetailType.Injector, true));
            _detailsCount.Add(RandomGenerator.GetRandomNumber(MinDetailsCount, MaxDetailsCount + 1));
            _storage.Add(new Detail(DetailType.Transmission, true));
            _detailsCount.Add(RandomGenerator.GetRandomNumber(MinDetailsCount, MaxDetailsCount + 1));
        }

        private void ShowError()
        {
            Console.WriteLine("Некоректное значение.");
        }
    }

    class Car
    {
        private List<Detail> _details = new List<Detail>();

        public Car(IReadOnlyList<string> detailsList)
        {
            int brokenDetail = RandomGenerator.GetRandomNumber(detailsList.Count);

            for (int i = 0; i < detailsList.Count; i++)
            {
                if (brokenDetail == i)
                    _details.Add(new Detail((DetailType)i, false));
                else
                    _details.Add(new Detail((DetailType)i, true));
            }
        }

        public bool TryFindBrokenDetail(out Detail brokenDetail)
        {
            foreach (var detail in _details)
                if (detail.IsWorking == false)
                {
                    brokenDetail = detail.Clone();
                    return true;
                }

            brokenDetail = null;
            return false;
        }

        public void ReplaceDetail(Detail newDetail)
        {
            foreach (var detail in _details)
            {
                if (detail.Type == newDetail.Type)
                {
                    _details.Remove(detail);
                    _details.Add(newDetail);
                    break;
                }
            }
        }
    }

    class Detail
    {
        public Detail(DetailType type, bool isWorking)
        {
            Type = type;
            IsWorking = isWorking;
        }

        public DetailType Type { get; private set; }
        public bool IsWorking { get; private set; }

        public Detail Clone() => new Detail(Type, IsWorking);
    }

    static class RandomGenerator
    {
        private static Random s_random = new Random();

        public static int GetRandomNumber(int minValue, int maxValue) => s_random.Next(minValue, maxValue);

        public static int GetRandomNumber(int maxValue) => s_random.Next(maxValue);
    }

    enum DetailType
    {
        Engine,
        Carburetor,
        Injector,
        Transmission,
    }
}