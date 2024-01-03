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
        private Storage _storage = new Storage();

        private Car _currentCar;
        private float _penaltyPercentage;
        private int _money;

        public CarService()
        {
            _penaltyPercentage = 0.15f;
            _money = 150;

            FillPriceLists();
        }

        private enum Menu
        {
            FindPartInStock,
            RefuseClient,
            RepairCar,
            Exit,
        }

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
                                _storage.ContainsDetail((DetailType)partIndex);
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

            for (int i = 0; i < _storage.DetailsList.Count; i++)
                Console.WriteLine($"{i + 1} - {_storage.DetailsList[i]}");

            Console.Write("\nВыберите номер детали для замены: ");

            if (int.TryParse(Console.ReadLine(), out int numberDetail))
            {
                numberDetail--;

                if (_storage.TryGetDetail(out Detail newDetail, (DetailType)numberDetail))
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
            char delimeterSymbol = '=';
            int delimeterLenght = 50;

            string delimeter = new string(delimeterSymbol, delimeterLenght);

            if (partIndex >= 0)
            {
                Console.WriteLine($"У клиента сломан: {_storage.DetailsList[partIndex]}.\tЦена детали: {_priceListDetails[partIndex]}" +
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
                for (int i = 0; i < _storage.DetailsList.Count; i++)
                    if (_storage.DetailsList[i].ToLower() == brokenDetail.Type.ToString().ToLower())
                        return i;

            return -1;
        }

        private Car GetNewClient() => new Car(_storage.DetailsList);

        private void FillPriceLists()
        {
            int maxPriceDetail = 200;
            int minPriceDetail = 55;
            int maxPriceWork = 100;
            int minPriceWork = 20;

            for (int i = 0; i < _storage.DetailsList.Count; i++)
            {
                _priceListDetails.Add(RandomGenerator.GetRandomNumber(minPriceDetail, maxPriceDetail + 1));
                _priceListWorks.Add(RandomGenerator.GetRandomNumber(minPriceWork, maxPriceWork + 1));
            }
        }

        private void ShowError()
        {
            Console.WriteLine("Некоректное значение.");
        }
    }

    class Storage
    {
        private List<int> _detailsCount = new List<int>();
        private List<Detail> _detailsName = new List<Detail>();
        private List<string> _detailsList;

        public Storage()
        {
            _detailsList = Enum.GetNames(typeof(DetailType)).ToList();
            FillStorage();
        }

        public IReadOnlyList<string> DetailsList => _detailsList;

        public void ContainsDetail(DetailType type)
        {
            bool isAvailable = false;

            foreach (var detail in _detailsName)
                if (detail.Type == type)
                    isAvailable = true;

            if (isAvailable)
                Console.WriteLine("Деталь есть в наличии.");
            else
                Console.WriteLine("Такой детали нет на складе.");
        }

        public bool TryGetDetail(out Detail detail, DetailType detailType)
        {
            for (int i = 0; i < _detailsName.Count; i++)
            {
                if (_detailsName[i].Type == detailType)
                {
                    _detailsCount[i]--;

                    if (_detailsCount[i] == 0)
                    {
                        _detailsCount.RemoveAt(i);
                        _detailsName.RemoveAt(i);
                    }

                    detail = new Detail(detailType, isWorking: true);
                    return true;
                }
            }

            detail = null;
            return false;
        }

        private void FillStorage()
        {
            int maxDetailsCount = 12;
            int minDetailsCount = 4;

            _detailsName.Add(new Detail(DetailType.Engine, isWorking: true));
            _detailsCount.Add(RandomGenerator.GetRandomNumber(minDetailsCount, maxDetailsCount + 1));
            _detailsName.Add(new Detail(DetailType.Carburetor, isWorking: true));
            _detailsCount.Add(RandomGenerator.GetRandomNumber(minDetailsCount, maxDetailsCount + 1));
            _detailsName.Add(new Detail(DetailType.Injector, isWorking: true));
            _detailsCount.Add(RandomGenerator.GetRandomNumber(minDetailsCount, maxDetailsCount + 1));
            _detailsName.Add(new Detail(DetailType.Transmission, isWorking: true));
            _detailsCount.Add(RandomGenerator.GetRandomNumber(minDetailsCount, maxDetailsCount + 1));
        }
    }

    class Car
    {
        private List<Detail> _details = new List<Detail>();

        public Car(IReadOnlyList<string> detailsList)
        {
            int brokenDetailIndex = RandomGenerator.GetRandomNumber(detailsList.Count);

            for (int i = 0; i < detailsList.Count; i++)
            {
                if (brokenDetailIndex == i)
                    _details.Add(new Detail((DetailType)i, isWorking: false));
                else
                    _details.Add(new Detail((DetailType)i, isWorking: true));
            }
        }

        public bool TryFindBrokenDetail(out Detail brokenDetail)
        {
            foreach (var detail in _details)
            {
                if (detail.IsWorking == false)
                {
                    brokenDetail = detail.Clone();
                    return true;
                }
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