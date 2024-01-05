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
            _priceList = new PriceList();
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
                    DetailType type = GetBrokenDetailType();
                    ShowMenu(type);
                    Console.Write("Выберите действие: ");

                    if (int.TryParse(Console.ReadLine(), out int number))
                    {
                        number--;

                        switch ((Menu)number)
                        {
                            case Menu.FindPartInStock:
                                SeeDetailAvailability(type);
                                break;

                            case Menu.RefuseClient:
                                RefuseClient(type);
                                break;

                            case Menu.RepairCar:
                                RepairCar(type);
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

        private void RepairCar(DetailType brokenDetail)
        {
            Console.Clear();

            for (int i = 0; i < _storage.DetailsList.Count; i++)
                Console.WriteLine($"{i + 1} - {_storage.DetailsList[i]}");

            Console.Write("\nВыберите тип детали для замены: ");

            if (Enum.TryParse(Console.ReadLine(), out DetailType type))
            {
                type--;

                if (_storage.TryGiveDetail(out Detail newDetail, type))
                {
                    if (brokenDetail == type)
                    {
                        _currentCar.ReplaceDetail(newDetail);
                        _money += _priceList.Details[type];
                        _money += _priceList.Works[type];
                        Console.WriteLine("Деталь успешна заменена.");
                    }
                    else
                    {
                        Console.Write("Вы заменили не ту деталь");
                        PayFine(type);
                    }

                    if (_storage.ContainsDetail((DetailType)type) == false)
                        _priceList.Remove(type);
                }
                else
                {
                    Console.Write($"На складе нет выбранной детали");
                    PayFine(type);
                }

                _currentCar = GetNewClient();
            }
            else
            {
                ShowError();
            }
        }

        private void PayFine(DetailType type)
        {
            int penalty = _priceList.Details[type] + _priceList.Works[type];
            _money -= penalty;
            Console.WriteLine($" и вынуждены возместить ушерб в размере: {penalty}.");
        }

        private void RefuseClient(DetailType type)
        {
            int penalty = (int)(_priceList.Details[type] * _penaltyPercentage);
            _money -= penalty;
            Console.WriteLine($"Вы отказали клиенту и вынуждены были оплатить штраф: {penalty}");

            _currentCar = GetNewClient();
        }

        private void ShowMenu(DetailType type)
        {
            char delimeterSymbol = '=';
            int delimeterLenght = 50;

            string delimeter = new string(delimeterSymbol, delimeterLenght);

            Console.WriteLine($"У клиента сломан: {_storage.DetailsList[(int)type]}.\tЦена детали: {_priceList.Details[type]}" +
                              $".\tЦена за замену: {_priceList.Works[type]}.\nВаш наличный баланс: {_money}.");

            Console.WriteLine(delimeter);
            Console.WriteLine($"{((int)Menu.FindPartInStock) + 1} - Найти деталь на складе.\n{((int)Menu.RefuseClient) + 1} - Отказать клиенту.\n" +
                              $"{((int)Menu.RepairCar) + 1} - Починить машину клиента.\n{((int)Menu.Exit) + 1} - Выйти.");
            Console.WriteLine(delimeter);
        }

        private DetailType GetBrokenDetailType()
        {
            Detail brokenDetail = _currentCar.GetBrokenDetail();

            for (int i = 0; i < _storage.DetailsList.Count; i++)
                if (_storage.DetailsList[i].ToLower() == brokenDetail.Type.ToString().ToLower())
                    return (DetailType)i;

            return (DetailType)(-1);
        }

        private Car GetNewClient() => new Car(_storage.DetailsList);

        private void ShowError()
        {
            Console.WriteLine("Некоректное значение.");
        }

        private class PriceList
        {
            private Dictionary<DetailType, int> _details = new Dictionary<DetailType, int>();
            private Dictionary<DetailType, int> _works = new Dictionary<DetailType, int>();

            public PriceList()
            {
                int maxPriceDetail = 200;
                int minPriceDetail = 55;
                int maxPriceWork = 100;
                int minPriceWork = 20;

                int detailsCount = Enum.GetValues(typeof(DetailType)).Length;

                for (int i = 0; i < detailsCount; i++)
                {
                    _details.Add((DetailType)i, RandomGenerator.GetRandomNumber(minPriceDetail, maxPriceDetail + 1));
                    _works.Add((DetailType)i, RandomGenerator.GetRandomNumber(minPriceWork, maxPriceWork + 1));
                }
            }

            public IReadOnlyDictionary<DetailType, int> Details => _details;
            public IReadOnlyDictionary<DetailType, int> Works => _works;

            public void Remove(DetailType type)
            {
                _details.Remove(type);
                _works.Remove(type);
            }
        }
    }

    class Storage
    {
        private List<Cell> _cells;
        private List<string> _detailsList;

        public Storage()
        {
            _detailsList = Enum.GetNames(typeof(DetailType)).ToList();
            FillStorage();
        }

        public IReadOnlyList<string> DetailsList => _detailsList;

        public bool ContainsDetail(DetailType type)
        {
            foreach (var cell in _cells)
                if (cell.DetailType == type)
                    return true;

            return false;
        }

        public bool TryGiveDetail(out Detail detail, DetailType detailType)
        {
            for (int i = 0; i < _cells.Count; i++)
            {
                if (_cells[i].DetailType == detailType)
                {
                    detail = _cells[i].GiveDetail();

                    if (_cells[i].DetailsCount == 0)
                        _cells.RemoveAt(i);

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
            _cells.Add(new Cell(new Detail(DetailType.Engine, isWorking: true), detailsCount));

            detailsCount = RandomGenerator.GetRandomNumber(minDetailsCount, maxDetailsCount + 1);
            _cells.Add(new Cell(new Detail(DetailType.Carburetor, isWorking: true), detailsCount));

            detailsCount = RandomGenerator.GetRandomNumber(minDetailsCount, maxDetailsCount + 1);
            _cells.Add(new Cell(new Detail(DetailType.Injector, isWorking: true), detailsCount));

            detailsCount = RandomGenerator.GetRandomNumber(minDetailsCount, maxDetailsCount + 1);
            _cells.Add(new Cell(new Detail(DetailType.Transmission, isWorking: true), detailsCount));
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

            _details[brokenDetailIndex].Break();
        }

        public Detail GetBrokenDetail()
        {
            foreach (var detail in _details)
                if (detail.IsWorking == false)
                    return detail.Clone();

            return null;
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

        public void Break() => IsWorking = false;
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