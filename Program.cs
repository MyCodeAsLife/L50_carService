using System;
using System.Collections.Generic;

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
        private IReadOnlyList<int> _detailsPrice;
        private List<Detail> _storage = new List<Detail>();
        private List<int> _workPrice = new List<int>();
        private DetailCreater _detailCreater = new DetailCreater();
        private Car _currentCar;

        private int _maxWorkPrice = 200;
        private int _minWorkPrice = 55;
        private int _maxDetail = 12;
        private int _minDetail = 4;

        private int _countEngine;
        private int _countCarburetor;
        private int _countInjector;
        private int _countTramsmission;

        public CarService()
        {
            FillPriceLists();
            FillStorage();
        }

        public void Run()   // Доделать
        {
            _currentCar = GetNewClient();
            int price;

            // Найти поломку -> определить цену
            DetailType brokenDetail = _currentCar.TryFindBrokenDetail();
            CarModel brokenDetailModel = _currentCar.Model;         // Нужна ли переменная?

            // Выбрать\найти деталь со склада

            // Отказать клиенту
            // Заменить деталь

        }

        public bool ContainsDetail(CarModel carModel, DetailType detailType)
        {
            foreach (var detail in _storage)
            {
                if (detail.Key == detailType)
                    if (detail.Value.CarModel == carModel)
                        return true;
            }

            return false;
        }

        public Detail GetDetail(CarModel carModel, DetailType detailType)       // Здесь 
        {
            Detail detail = null;

            if (ContainsDetail(carModel, detailType))
            {
                if (detailType == DetailType.Engine)
                    _countEngine--;
                else if (detailType == DetailType.Carburetor)
                    _countCarburetor--;
                else if (detailType == DetailType.Injector)
                    _countInjector--;
                else
                    _countTramsmission--;

                // 
            }

            return detail;
        }

        private Car GetNewClient()                                                                  // Переделать
        {
            return new Car((CarModel)RandomGenerator.GetRandomNumber((int)CarModel.Max));
        }

        private void FillPriceLists()
        {
            _detailsPrice = _detailCreater.DetailsPrice;

            for (int i = 0; i < _detailsPrice.Count; i++)
                _workPrice.Add(RandomGenerator.GetRandomNumber(_minWorkPrice, _maxWorkPrice + 1));
        }

        private void FillStorage()                                                                  // Переделать
        {
            _countEngine = RandomGenerator.GetRandomNumber(_minDetail, _maxDetail + 1);
            _countCarburetor = RandomGenerator.GetRandomNumber(_minDetail, _maxDetail + 1);
            _countInjector = RandomGenerator.GetRandomNumber(_minDetail, _maxDetail + 1);
            _countTramsmission = RandomGenerator.GetRandomNumber(_minDetail, _maxDetail + 1);

            CreateDetails(DetailType.Engine, _countEngine);
            CreateDetails(DetailType.Carburetor, _countCarburetor);
            CreateDetails(DetailType.Injector, _countInjector);
            CreateDetails(DetailType.Transmission, _countTramsmission);
        }

        private void CreateDetails(DetailType type, int count)                                                                  // Переделать
        {
            Detail newDetail = null;
            bool isWorking = true;

            for (int i = 0; i < count; i++)
            {
                switch (type)
                {
                    case DetailType.Engine:
                        _storage(new Engine((CarModel)RandomGenerator.GetRandomNumber((int)CarModel.Max), isWorking);         // Отдельный метод по созданию деталей в класс Деталей?
                        break;

                    case DetailType.Carburetor:
                        newDetail = new Carburetor((CarModel)RandomGenerator.GetRandomNumber((int)CarModel.Max), isWorking);      // Отдельный метод по созданию деталей в класс Деталей?
                        break;

                    case DetailType.Injector:
                        newDetail = new Injector((CarModel)RandomGenerator.GetRandomNumber((int)CarModel.Max), isWorking);        // Отдельный метод по созданию деталей в класс Деталей?
                        break;

                    case DetailType.Transmission:
                        newDetail = new Transmission((CarModel)RandomGenerator.GetRandomNumber((int)CarModel.Max), isWorking);        // Отдельный метод по созданию деталей в класс Деталей?
                        break;

                    default:
                        ShowError();
                        break;
                }

                _storage.Add(type, newDetail);
            }
        }

        // Добавить генерато машин(с деталями) одна из деталей сломана.

        public static void ShowError()          // Удалить если 1 вызов
        {
            Console.WriteLine("Некоректное значение.");
        }
    }

    class Car
    {
        private List<Detail> _details;

        public Car(CarModel model, List<Detail> details)
        {
            Model = model;
            _details = details;
        }

        public CarModel Model { get; private set; }

        public bool TryFindBrokenDetail(out Detail brokenDetail)
        {
            brokenDetail = null;

            foreach (var detail in _details)
            {
                if (detail.CheckServiceability() == false)
                {
                    brokenDetail = detail;
                    return true;
                }
            }

            return false;
        }
    }

    class Detail
    {
        private bool _isWorking;

        public Detail(DetailType type, CarModel model, int price, bool isWorking)
        {
            Type = type;
            CarModel = model;
            Price = price;
            _isWorking = isWorking;
        }

        public CarModel CarModel { get; private set; }
        public DetailType Type { get; private set; }
        public int Price { get; private set; }

        public bool CheckServiceability() => _isWorking;
    }

    class DetailCreater
    {
        private List<DetailType> _listDetails = new List<DetailType>();         // Тут нужно?
        private List<CarModel> _listCarModels = new List<CarModel>();         // Тут нужно?
        private List<int> _detailsPrice = new List<int>();

        private int _minPrice = 50;
        private int _maxPrice = 300;

        public DetailCreater()
        {
            var tempListCarModels = Enum.GetValues(typeof(CarModel));
            var tempListDetails = Enum.GetValues(typeof(DetailType));

            foreach (var carModel in tempListCarModels)
                _listCarModels.Add((CarModel)carModel);

            foreach (var detail in tempListDetails)
            {
                _listDetails.Add((DetailType)detail);
                _detailsPrice.Add(RandomGenerator.GetRandomNumber(_minPrice, _maxPrice + 1));
            }
        }

        public IReadOnlyList<DetailType> ListDetails => _listDetails;                   // Удалить?

        public IReadOnlyList<CarModel> ListCarModels => _listCarModels;                   // Удалить?

        public IReadOnlyList<int> DetailsPrice => _detailsPrice;

        public Detail Create(DetailType type, CarModel model)
        {
            int index = _listDetails.FindIndex(detailType => detailType == type);                                                 // Проверить работоспособность
            return new Detail(type, model, _detailsPrice[index], true);
        }
    }

    static class RandomGenerator
    {
        private static Random s_random = new Random();

        public static int GetRandomNumber(int minValue, int maxValue) => s_random.Next(minValue, maxValue);

        public static int GetRandomNumber(int maxValue) => s_random.Next(maxValue);
    }

    enum CarModel
    {
        Ford,
        Reno,
        Audi,
        BMW,
    }

    enum DetailType
    {
        Engine,
        Carburetor,
        Injector,
        Transmission,
    }
}