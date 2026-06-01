export class ProductModel
{
    title:string;
    price:number;
    description:string;
    thumbnailImage:string;

    constructor(title:string = "Macbook Pro", price:number = 100000, public stock:number = 10, description:string = "A great laptop with good spec and reviews! Consider buying it", thumbnailImage:string = "https://5.imimg.com/data5/SELLER/Default/2021/11/JO/DF/OI/74357280/apple-macbook-pro-500x500.jpg")
    {
        this.title = title;
        this.price = price;
        this.description = description;
        this.thumbnailImage = thumbnailImage;
    }
}