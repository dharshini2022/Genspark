export class ProductModel
{
    id:number;
    title:string;
    price:number;
    stock:number;
    description:string;
    thumbnail:string;


    //public stock = 10, creates stock at obj initialisation
    constructor(id:number = 0, title:string = "Macbook Pro", stock:number = 10, price:number = 100000, description:string = "A great laptop with good spec and reviews! Consider buying it", thumbnail:string = "https://5.imimg.com/data5/SELLER/Default/2021/11/JO/DF/OI/74357280/apple-macbook-pro-500x500.jpg")
    {
        this.id = id;
        this.title = title;
        this.stock = stock;
        this.price = price;
        this.description = description;
        this.thumbnail = thumbnail;
    }
}