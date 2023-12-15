using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L50_carService
{
    internal class Program
    {
        static void Main(string[] args)
        {
        }
    }

    class CarService
    {
        private Dictionary<DetailType, Detail> _storage = new Dictionary<DetailType, Detail>();     // Переделать под массив, и при поиске у деталей запрашивать (тип детали и тип автомобиля)
        //private Dictionary<int, int> _priceList = new Dictionary<int, int>();
        private int[,] _priceList = new int[(int)CarModel.Max, (int)DetailType.Max];
        // Создасть словарь деталей?
        private Car _currentCar;
        private Random _random;

        private int _maxPrice = 200;
        private int _minPrice = 55;
        private int _maxDetail = 12;
        private int _minDetail = 4;

        private int _countEngine;
        private int _countCarburetor;
        private int _countInjector;
        private int _countTramsmission;

        public CarService(Random random)
        {
            _random = random;

            FillStorage();
            FillPriceList();
        }

        public void Run()   // Доделать
        {
            _currentCar = GetNewClient();
            int price;

            // Найти поломку -> определить цену
            DetailType brokenDetail = _currentCar.FindBreakdown();
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
                else if(detailType == DetailType.Carburetor)
                    _countCarburetor--;
                else if( detailType == DetailType.Injector)
                    _countInjector--;
                else
                    _countTramsmission--;

                    // 
            }

            return detail;
        }

        private Car GetNewClient()
        {
            return new Car((CarModel)_random.Next((int)CarModel.Max), _random);
        }

        private void FillPriceList()
        {
            for (int i = 0; i < (int)CarModel.Max; i++)
                for (int j = 0; j < (int)DetailType.Max; j++)
                    _priceList[i, j] = _random.Next(_minPrice, _maxPrice + 1);
        }

        private void FillStorage()
        {
            _countEngine = _random.Next(_minDetail, _maxDetail + 1);
            _countCarburetor = _random.Next(_minDetail, _maxDetail + 1);
            _countInjector = _random.Next(_minDetail, _maxDetail + 1);
            _countTramsmission = _random.Next(_minDetail, _maxDetail + 1);

            CreateDetails(DetailType.Engine, _countEngine);
            CreateDetails(DetailType.Carburetor, _countCarburetor);
            CreateDetails(DetailType.Injector, _countInjector);
            CreateDetails(DetailType.Transmission, _countTramsmission);
        }

        private void CreateDetails(DetailType type, int count)
        {
            Detail newDetail = null;
            bool isWorking = true;

            for (int i = 0; i < count; i++)
            {
                switch (type)
                {
                    case DetailType.Engine:
                        newDetail = new Engine((CarModel)_random.Next((int)CarModel.Max), isWorking);         // Отдельный метод по созданию деталей в класс Деталей?
                        break;

                    case DetailType.Carburetor:
                        newDetail = new Carburetor((CarModel)_random.Next((int)CarModel.Max), isWorking);      // Отдельный метод по созданию деталей в класс Деталей?
                        break;

                    case DetailType.Injector:
                        newDetail = new Injector((CarModel)_random.Next((int)CarModel.Max), isWorking);        // Отдельный метод по созданию деталей в класс Деталей?
                        break;

                    case DetailType.Transmission:
                        newDetail = new Transmission((CarModel)_random.Next((int)CarModel.Max), isWorking);        // Отдельный метод по созданию деталей в класс Деталей?
                        break;

                    default:
                        Error.Show();
                        break;
                }

                _storage.Add(type, newDetail);
            }
        }
    }

    class Car
    {
        private Random _random;
        private Dictionary<DetailType, Detail> _mechanism = new Dictionary<DetailType, Detail>();
        //private CarModel Model;

        public Car(CarModel model, Random random)
        {
            Model = model;
            _random = random;

            int brokenDetail = _random.Next((int)DetailType.Max);
            bool isWorking = true;

            for (int i = 0; i < (int)DetailType.Max; i++)
            {
                if (brokenDetail == i)
                    isWorking = false;

                switch ((DetailType)i)
                {
                    case DetailType.Engine:
                        _mechanism.Add((DetailType)i, new Engine(Model, isWorking));         // Отдельный метод по созданию деталей в класс Деталей?, добавление в механизм вынести из switch
                        break;

                    case DetailType.Carburetor:
                        _mechanism.Add((DetailType)i, new Carburetor(Model, isWorking));         // Отдельный метод по созданию деталей в класс Деталей?, добавление в механизм вынести из switch
                        break;

                    case DetailType.Injector:
                        _mechanism.Add((DetailType)i, new Injector(Model, isWorking));       // Отдельный метод по созданию деталей в класс Деталей?, добавление в механизм вынести из switch
                        break;

                    case DetailType.Transmission:
                        _mechanism.Add((DetailType)i, new Transmission(Model, isWorking));       // Отдельный метод по созданию деталей в класс Деталей?, добавление в механизм вынести из switch
                        break;

                    default:
                        Error.Show();
                        break;
                }
            }
        }

        public CarModel Model { get; private set; }

        public DetailType FindBreakdown()
        {
            DetailType brokenDetail = DetailType.Max;

            foreach (var detail in _mechanism)
            {
                detail.Value.Work();

                if (detail.Value.CheckServiceability() == false)
                    brokenDetail = detail.Key;
            }

            return brokenDetail;
        }
    }

    abstract class Detail
    {
        //private CarModel CarModel;
        private bool _isWorking;

        public Detail(CarModel model, bool isWorking)
        {
            CarModel = model;
            _isWorking = isWorking;
        }

        public CarModel CarModel { get; private set; }

        public abstract void Work();

        public bool CheckServiceability()
        {
            return _isWorking;
        }

        public abstract DetailType GetDetailType();         // Дублирует имя класса потому как GetType занята классом object
    }

    class Engine : Detail
    {
        public Engine(CarModel model, bool isWorking) : base(model, isWorking) { }

        public override void Work()
        {
            if (CheckServiceability())
                Console.WriteLine("Двигатель мягко гудит.");
            else
                Console.WriteLine("В двигателе чтото стучит.");
        }

        public override DetailType GetDetailType()
        {
            return DetailType.Engine;
        }
    }

    class Carburetor : Detail
    {
        public Carburetor(CarModel model, bool isWorking) : base(model, isWorking) { }

        public override void Work()     // Переписать
        {
            if (CheckServiceability())
                Console.WriteLine("Двигатель мягко гудит.");
            else
                Console.WriteLine("В двигателе чтото стучит.");
        }

        public override DetailType GetDetailType()
        {
            return DetailType.Carburetor;
        }
    }

    class Injector : Detail
    {
        public Injector(CarModel model, bool isWorking) : base(model, isWorking) { }

        public override void Work()     // Переписать
        {
            if (CheckServiceability())
                Console.WriteLine("Двигатель мягко гудит.");
            else
                Console.WriteLine("В двигателе чтото стучит.");
        }

        public override DetailType GetDetailType()
        {
            return DetailType.Injector;
        }
    }

    class Transmission : Detail
    {
        public Transmission(CarModel model, bool isWorking) : base(model, isWorking) { }

        public override void Work()     // Переписать
        {
            if (CheckServiceability())
                Console.WriteLine("Двигатель мягко гудит.");
            else
                Console.WriteLine("В двигателе чтото стучит.");
        }

        public override DetailType GetDetailType()
        {
            return DetailType.Transmission;
        }
    }

    class Error
    {
        public static void Show()
        {
            Console.WriteLine("Некоректное значение.");
        }
    }

    enum CarModel
    {
        Ford,
        Reno,
        Audi,
        BMW,
        Max,
    }

    enum DetailType
    {
        Engine,
        Carburetor,
        Injector,
        Transmission,
        Max,
    }
}