using System;
using System.Collections.Generic;

public class Good
{

    public Good(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название товара не может быть пустым", nameof(name));

        Name = name;
    }

    public string Name { get; }
}

public interface IWareHouse
{
    public void Delive(Good good, int count);
    public bool HasEnough(Good good, int count);
    public void Reserve(Good good, int count);
    public void PrintStock();
}

public class WareHouse : IWareHouse
{
    private Dictionary<string, int> _stock = new();

    public void Delive(Good good, int count)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count),"Недостаочно товаров");

        if (_stock.ContainsKey(good.Name))
            _stock[good.Name] += count;
        else
            _stock[good.Name] = count;
    }

    public bool HasEnough(Good good, int count)
    {
        return _stock.TryGetValue(good.Name, out int available) && available >= count;
    }

    public void Reserve(Good good, int count)
    {
        if (HasEnough(good, count) == false)
            throw new InvalidOperationException($"Недостаточно товара {good.Name} на складе");

        _stock[good.Name] -= count;
    }

    public void PrintStock()
    {
        Console.WriteLine("Общее количество товаров на складе");

        foreach (var pair in _stock)
            Console.WriteLine($"{pair.Key}: {pair.Value} штук.");

    }
}

public class Cart
{
    private readonly Dictionary<string, int> _items = new();
    private readonly IWareHouse _warehouse;

    public Cart(IWareHouse warehouse)
    {

        if (warehouse == null)
            throw new ArgumentNullException(nameof(warehouse), "Нет склада");

        _warehouse = warehouse;
    }

    public void Add(Good good, int count)
    {
        if (_warehouse.HasEnough(good, count)==false)
            throw new InvalidOperationException($"Недостаточно товара {good.Name} на складе");

        if (_items.ContainsKey(good.Name))
            _items[good.Name] += count;
        else
            _items[good.Name] = count;
    }

    public Order Order()
    {
        foreach (var pair in _items)
        {
            Good good = new Good(pair.Key); 
            _warehouse.Reserve(good, pair.Value);
        }

        return new Order("https://payment.com/your-order-123");
    }

    public void PrintCart()
    {
        Console.WriteLine("Товары в корзине");

        foreach (var pair in _items)
            Console.WriteLine($"{pair.Key}: {pair.Value} штук");

    }
}

public class Order
{

    public Order(string paylink)
    {

        if(paylink == null)
            throw new ArgumentNullException(nameof(paylink), "Нет ссылки на оплату");

        Paylink = paylink;
    }

    public string Paylink { get; }
}

public class Shop
{
    private IWareHouse _warehouse;

    public Shop(IWareHouse warehouse)
    {

        if (warehouse == null)
            throw new ArgumentNullException(nameof(warehouse),"Нет склада");

        _warehouse = warehouse;
    }

    public Cart Cart()
    {

        if (_warehouse == null)
            throw new ArgumentNullException(nameof(_warehouse), "Нет склада");

        return new Cart(_warehouse);
    }

}
public class OnlineShopping
{
    public void Run()
    {
        Good iPhone12 = new Good("IPhone 12");
        Good iPhone11 = new Good("IPhone 11");

        WareHouse warehouse = new WareHouse();

        Shop shop = new Shop(warehouse);

        warehouse.Delive(iPhone12, 10);
        warehouse.Delive(iPhone11, 1);

        //Вывод всех товаров на складе с их остатком

        Cart cart = shop.Cart();
        cart.Add(iPhone12, 4);
        cart.Add(iPhone11, 3); //при такой ситуации возникает ошибка так, как нет нужного количества товара на складе

        //Вывод всех товаров в корзине

        Console.WriteLine(cart.Order().Paylink);

        cart.Add(iPhone12, 9); //Ошибка, после заказа со склада убираются заказанные товары
    }
}
