using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;

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
        private Storage _storage = new Storage();
        private PriceList _priceList;

        private Car _currentCar;
        private float _penaltyPercentage;
        private int _money;

        public CarService()
        {
            _penaltyPercentage = 0.15f;
            _money = 150;
            _priceList = new PriceList(_storage.DetailsList.Count);
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
                                SeeDetailAvailability((DetailType)partIndex);
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

        private void SeeDetailAvailability(DetailType type)
        {
            if (_storage.ContainsDetail(type))
                Console.WriteLine("Деталь есть в наличии.");
            else
                Console.WriteLine("Такой детали нет на складе.");
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

                if (_storage.TryGiveDetail(out Detail newDetail, (DetailType)numberDetail))
                {
                    if (indexBrokenDetail == numberDetail)
                    {
                        _currentCar.ReplaceDetail(newDetail);
                        _money += _priceList.Details[numberDetail];
                        _money += _priceList.Works[numberDetail];
                        Console.WriteLine("Деталь успешна заменена.");
                    }
                    else
                    {
                        Console.Write("Вы заменили не ту деталь");
                        PayFine(numberDetail);
                    }

                    if (_storage.ContainsDetail((DetailType)numberDetail) == false)
                        _priceList.RemoveAt(numberDetail);
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
            int penalty = _priceList.Details[detailIndex] + _priceList.Works[detailIndex];
            _money -= penalty;
            Console.WriteLine($" и вынуждены возместить ушерб в размере: {penalty}.");
        }

        private void RefuseClient(int partIndex)
        {
            int penalty = (int)(_priceList.Details[partIndex] * _penaltyPercentage);
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
                Console.WriteLine($"У клиента сломан: {_storage.DetailsList[partIndex]}.\tЦена детали: {_priceList.Details[partIndex]}" +
                                  $".\tЦена за замену: {_priceList.Works[partIndex]}.\nВаш наличный баланс: {_money}.");
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

        private void ShowError()
        {
            Console.WriteLine("Некоректное значение.");
        }

        private class PriceList
        {
            private List<int> _details = new List<int>();
            private List<int> _works = new List<int>();

            public PriceList(int detailsCount)
            {
                int maxPriceDetail = 200;
                int minPriceDetail = 55;
                int maxPriceWork = 100;
                int minPriceWork = 20;

                for (int i = 0; i < detailsCount; i++)
                {
                    _details.Add(RandomGenerator.GetRandomNumber(minPriceDetail, maxPriceDetail + 1));
                    _works.Add(RandomGenerator.GetRandomNumber(minPriceWork, maxPriceWork + 1));
                }
            }

            public IReadOnlyList<int> Details => _details;
            public IReadOnlyList<int> Works => _works;

            public void RemoveAt(int index)
            {
                if (index < _details.Count && index >= 0)
                {
                    _details.RemoveAt(index);
                    _works.RemoveAt(index);
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    class Storage
    {
        private List<Cell> _storage;
        private List<string> _detailsList;

        public Storage()
        {
            _detailsList = Enum.GetNames(typeof(DetailType)).ToList();
            FillStorage();
        }

        public IReadOnlyList<string> DetailsList => _detailsList;

        public bool ContainsDetail(DetailType type)
        {
            foreach (var cell in _storage)
                if (cell.DetailType == type)
                    return true;

            return false;
        }

        public bool TryGiveDetail(out Detail detail, DetailType detailType)
        {
            for (int i = 0; i < _storage.Count; i++)
            {
                if (_storage[i].DetailType == detailType)
                {
                    detail = _storage[i].GiveDetail();

                    if (_storage[i].DetailsCount == 0)
                        _storage.RemoveAt(i);

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
            int detailsCount;

            detailsCount = RandomGenerator.GetRandomNumber(minDetailsCount, maxDetailsCount + 1);
            _storage.Add(new Cell(new Detail(DetailType.Engine, isWorking: true), detailsCount));

            detailsCount = RandomGenerator.GetRandomNumber(minDetailsCount, maxDetailsCount + 1);
            _storage.Add(new Cell(new Detail(DetailType.Carburetor, isWorking: true), detailsCount));

            detailsCount = RandomGenerator.GetRandomNumber(minDetailsCount, maxDetailsCount + 1);
            _storage.Add(new Cell(new Detail(DetailType.Injector, isWorking: true), detailsCount));

            detailsCount = RandomGenerator.GetRandomNumber(minDetailsCount, maxDetailsCount + 1);
            _storage.Add(new Cell(new Detail(DetailType.Transmission, isWorking: true), detailsCount));
        }

        private class Cell
        {
            private Detail _detail;
            private int _detailsCount;

            public Cell(Detail detail, int detailsCount)
            {
                _detail = detail;
                _detailsCount = detailsCount;
            }

            public DetailType DetailType => _detail.Type;
            public int DetailsCount => _detailsCount;

            public Detail GiveDetail()
            {
                _detailsCount--;
                return new Detail(_detail);
            }
        }
    }

    class Car
    {
        private List<Detail> _details = new List<Detail>();

        public Car(IReadOnlyList<string> detailsList)
        {
            int brokenDetailIndex = RandomGenerator.GetRandomNumber(detailsList.Count);

            for (int i = 0; i < detailsList.Count; i++)
                _details.Add(new Detail((DetailType)i, isWorking: true));

            _details[brokenDetailIndex].BreakDetail();
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

        public Detail(Detail detail)
        {
            Type = detail.Type;
            IsWorking = detail.IsWorking;
        }

        public DetailType Type { get; private set; }
        public bool IsWorking { get; private set; }

        public Detail Clone() => new Detail(Type, IsWorking);

        public void BreakDetail() => IsWorking = false;
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