INSERT INTO public.cart_items VALUES (45, 4, 34, 1);
INSERT INTO public.cart_items VALUES (46, 4, 28, 2);
INSERT INTO public.cart_items VALUES (49, 4, 19, 4);


--
-- TOC entry 4039 (class 0 OID 18668)
-- Dependencies: 228
-- Data for Name: carts; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.carts VALUES (3, 18, '2026-07-08 12:57:19.560473', NULL, NULL);
INSERT INTO public.carts VALUES (1, 4, '2026-06-16 14:21:46.794425', NULL, NULL);
INSERT INTO public.carts VALUES (4, 21, '2026-07-07 07:51:58.503702', '2026-07-07 07:51:58.503701', '6XRJW9QT');
INSERT INTO public.carts VALUES (2, 16, '2026-06-16 17:17:10.84908', NULL, NULL);


--
-- TOC entry 4035 (class 0 OID 18628)
-- Dependencies: 222
-- Data for Name: categories; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.categories VALUES (4, 'laptops', 1, true, 'electronics-laptops');
INSERT INTO public.categories VALUES (7, 'tv', 1, true, 'electronics-tv');
INSERT INTO public.categories VALUES (8, 'Audio Devices', 1, true, 'electronics-audio devices');
INSERT INTO public.categories VALUES (3, 'mobiles', 1, true, 'electronics-mobiles');
INSERT INTO public.categories VALUES (6, 'apple', 3, false, 'electronics-mobiles-apple');
INSERT INTO public.categories VALUES (9, 'books', NULL, true, 'books');
INSERT INTO public.categories VALUES (10, 'beauty', NULL, true, 'beauty');
INSERT INTO public.categories VALUES (2, 'fashion', NULL, true, 'fashion');
INSERT INTO public.categories VALUES (1, 'electronics', NULL, true, 'electronics');
INSERT INTO public.categories VALUES (5, 'android', 3, false, 'electronics-mobiles-andriod');
INSERT INTO public.categories VALUES (11, 'home decor', NULL, true, 'home-decor');
INSERT INTO public.categories VALUES (12, 'grocery', NULL, false, 'grocery');


--
-- TOC entry 4049 (class 0 OID 18795)
-- Dependencies: 242
-- Data for Name: discounts; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.discounts VALUES (3, NULL, NULL, 2, 'PW469XEA', 'Percentage', 5.0, 1500, 20, 0, true, '2026-07-10 00:00:00', 'Category', 0);
INSERT INTO public.discounts VALUES (2, NULL, 2, NULL, 'WSQBA4CE', 'Percentage', 10.0, 1000, 50, 0, true, '2026-06-11 10:12:15.659391', 'Product', 0);
INSERT INTO public.discounts VALUES (4, NULL, 1, NULL, 'QJL8E7UB', 'Percentage', 10.0, 10000, 50, 0, true, '2026-07-10 00:00:00', 'Product', 0);
INSERT INTO public.discounts VALUES (6, NULL, 7, NULL, 'AYE32J8K', 'Flat', 1000.0, 10000, 50, 0, true, '2026-07-10 00:00:00', 'Product', 0);
INSERT INTO public.discounts VALUES (7, NULL, NULL, 7, '36VMKXR9', 'Percentage', 2.0, 1500, 20, 0, true, '2026-07-10 00:00:00', 'Category', 0);
INSERT INTO public.discounts VALUES (9, NULL, 14, NULL, 'TJFMVM4M', 'Flat', 1000.0, 10000, 50, 1, true, '2026-08-10 00:00:00', 'Product', 0);
INSERT INTO public.discounts VALUES (8, NULL, NULL, 1, 'S26GVAPE', 'Percentage', 5.0, 15000, 20, 3, true, '2026-08-10 00:00:00', 'Category', 0);
INSERT INTO public.discounts VALUES (1, 1, NULL, NULL, 'YB5KXAAC', 'Flat', 100.0, 1000, 50, 0, false, '2026-07-05 17:31:18.289825', 'Vendor', 0);
INSERT INTO public.discounts VALUES (5, 7, NULL, NULL, '6XRJW9QT', 'Flat', 100.0, 1000, 50, 0, true, '2026-07-10 00:00:00', 'Vendor', 0);
INSERT INTO public.discounts VALUES (11, NULL, 13, NULL, 'AK8BMZP9', 'Flat', 1000, 10000, 10, 0, true, '2026-07-31 00:00:00', 'Product', 0);
INSERT INTO public.discounts VALUES (12, NULL, NULL, NULL, '3GYUESRK', 'Percentage', 5, 200000, 10, 0, true, '2026-07-25 00:00:00', 'Common', 0);
INSERT INTO public.discounts VALUES (13, 4, NULL, NULL, 'A9M2Y3F8', 'Flat', 500, 10000, 10, 0, true, '2026-07-24 00:00:00', 'Vendor', 0);
INSERT INTO public.discounts VALUES (10, 1, NULL, NULL, '8Z865ZYB', 'Flat', 1000, 20000, 10, 2, true, '2026-07-31 00:00:00', 'Vendor', 0);


--
-- TOC entry 4041 (class 0 OID 18682)
-- Dependencies: 230
-- Data for Name: notifications; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.notifications VALUES (1, 18, 'OrderPlaced', 'Success', 'Order Confirmed', 'Your order #39 of amount ₹66950.00 has been successfully paid and confirmed!', true, '2026-07-08 09:45:43.303196');
INSERT INTO public.notifications VALUES (2, 18, 'OrderPlaced', 'Success', 'Order Confirmed', 'Your order #40 of amount ₹92700.00 has been successfully paid and confirmed!', false, '2026-07-08 12:57:19.603215');


--
-- TOC entry 4057 (class 0 OID 19042)
-- Dependencies: 260
-- Data for Name: product_images; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.product_images VALUES (1, 1, 'https://www.google.com/url?sa=t&source=web&rct=j&url=https%3A%2F%2Fwww.idownloadblog.com%2F2024%2F03%2F04%2Fapple-macbook-air-midnight-color-anodization-process%2F&ved=0CBYQjRxqGAoTCPjQkc3q9ZQDFQAAAAAdAAAAABCPAQ&opi=89978449', 1);
INSERT INTO public.product_images VALUES (2, 1, 'https://www.google.com/url?sa=t&source=web&rct=j&url=https%3A%2F%2F9to5mac.com%2F2024%2F03%2F04%2Fmidnight-m3-macbook-air-fingerpints%2F&ved=0CBYQjRxqGAoTCPjQkc3q9ZQDFQAAAAAdAAAAABCqAQ&opi=89978449', 2);
INSERT INTO public.product_images VALUES (10, 16, 'https://shopatsc.com/cdn/shop/files/01-43S20-Primary-Image.jpg?v=1756188173', 1);
INSERT INTO public.product_images VALUES (12, 16, 'https://mb.cision.com/Public/2017/9517436/86f883fde7def966_800x800ar.jpg', 3);
INSERT INTO public.product_images VALUES (13, 16, 'https://cdn.mos.cms.futurecdn.net/XpsztyBauhTAvzqmFvyMP9.jpg', 2);
INSERT INTO public.product_images VALUES (14, 17, 'https://www.shutterstock.com/image-photo/kyiv-ukraine-september-9-2024-260nw-2518084593.jpg', 1);
INSERT INTO public.product_images VALUES (15, 17, 'https://png.pngtree.com/png-vector/20250416/ourmid/pngtree-white-airpods-wireless-earphones-with-charging-case-isolated-on-transparent-background-png-image_16032402.png', 2);
INSERT INTO public.product_images VALUES (16, 17, 'https://m.media-amazon.com/images/I/517YGi45KhL.jpg', 3);
INSERT INTO public.product_images VALUES (17, 18, 'https://static.vecteezy.com/system/resources/thumbnails/074/008/327/small/wireless-earbuds-earphones-headset-headphones-pods-audio-music-listening-device-photo.jpg', 3);
INSERT INTO public.product_images VALUES (18, 18, 'https://encrypted-tbn0.gstatic.com/shopping?q=tbn:ANd9GcRBJE7pE6UmY3u_BpxMvN7lTcuWW71CssTSFyLQK54ELwT0Su4', 1);
INSERT INTO public.product_images VALUES (19, 18, 'https://encrypted-tbn3.gstatic.com/shopping?q=tbn:ANd9GcRxC5cgUCSomB-vB8M7ToKGZ1ET1mS8OA7otS0xlxBOQZPNChk', 2);
INSERT INTO public.product_images VALUES (20, 19, 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSToK4elvMflHiJqksQ1-Pxn269EKa5UfId1lK28mDpMw&s=10', 1);
INSERT INTO public.product_images VALUES (21, 19, 'https://5.imimg.com/data5/SELLER/Default/2023/12/367535836/YO/UE/OT/204318408/sony-earth-blue-link-buds-s-ls900n-bluetooth-earbuds-500x500.jpg', 2);
INSERT INTO public.product_images VALUES (22, 19, 'https://m.media-amazon.com/images/I/81joUZ9KZzL._AC_UF350,350_QL80_.jpg', 3);
INSERT INTO public.product_images VALUES (23, 20, 'https://m.media-amazon.com/images/I/61oCISLE+PL._SL1500_.jpg', 1);
INSERT INTO public.product_images VALUES (24, 20, 'https://m.media-amazon.com/images/I/51pOjb8cWPL._SL1500_.jpg', 2);
INSERT INTO public.product_images VALUES (25, 20, 'https://m.media-amazon.com/images/I/61VVKSFncsL._SL1500_.jpg', 3);
INSERT INTO public.product_images VALUES (26, 21, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/headphone/s/r/9/bs-ultrapood-bullstorm-original-imahcus46hzumv9a.jpeg?q=90', 1);
INSERT INTO public.product_images VALUES (27, 21, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/headphone/w/b/l/bs-ultrapood-bullstorm-original-imahcus4z6xthxxs.jpeg?q=90', 2);
INSERT INTO public.product_images VALUES (28, 21, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/headphone/l/o/z/bs-ultrapood-bullstorm-original-imahcus4ttwfkash.jpeg?q=90', 3);
INSERT INTO public.product_images VALUES (29, 22, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mouse/y/4/y/-enriched-transparent-original-imahbg3mymavv86e.png?q=90', 1);
INSERT INTO public.product_images VALUES (30, 22, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mouse/q/5/e/-original-imahbg3mvqq2hnwv.jpeg?q=90', 2);
INSERT INTO public.product_images VALUES (31, 22, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mouse/d/e/g/-original-imahbg3mreybvrtk.jpeg?q=90', 3);
INSERT INTO public.product_images VALUES (32, 23, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mouse/f/v/6/-enriched-transparent-original-imahbg3nc2upghsp.png?q=90', 1);
INSERT INTO public.product_images VALUES (33, 23, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mouse/y/k/s/-original-imahbg3nk5fakwf5.jpeg?q=90', 2);
INSERT INTO public.product_images VALUES (34, 23, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mouse/6/u/2/-original-imahbg3ncu9j75tj.jpeg?q=90', 3);
INSERT INTO public.product_images VALUES (38, 25, 'https://m.media-amazon.com/images/I/51bfIuFGCAL._SL1080_.jpg', 1);
INSERT INTO public.product_images VALUES (39, 25, 'https://m.media-amazon.com/images/I/81AI1WLeeZL._SL1500_.jpg', 2);
INSERT INTO public.product_images VALUES (40, 25, 'https://m.media-amazon.com/images/I/71Z7NVPMLqL._SL1500_.jpg', 3);
INSERT INTO public.product_images VALUES (41, 26, 'https://m.media-amazon.com/images/I/713sRNYnBPL._SL1500_.jpg', 3);
INSERT INTO public.product_images VALUES (42, 26, 'https://m.media-amazon.com/images/I/61FIQV-BUxL._SL1500_.jpg', 2);
INSERT INTO public.product_images VALUES (43, 26, 'https://m.media-amazon.com/images/I/61HuMK8SFHL._SL1500_.jpg', 1);
INSERT INTO public.product_images VALUES (44, 27, 'https://m.media-amazon.com/images/I/61HuMK8SFHL._SL1500_.jpg', 1);
INSERT INTO public.product_images VALUES (45, 27, 'https://m.media-amazon.com/images/I/713sRNYnBPL._SL1500_.jpg', 2);
INSERT INTO public.product_images VALUES (46, 28, 'https://m.media-amazon.com/images/I/61x3nRatR9L._SL1500_.jpg', 1);
INSERT INTO public.product_images VALUES (47, 28, 'https://m.media-amazon.com/images/I/71gzyygFC3L._SL1500_.jpg', 2);
INSERT INTO public.product_images VALUES (48, 28, 'https://m.media-amazon.com/images/I/61D0dvR6WKL._SL1500_.jpg', 3);
INSERT INTO public.product_images VALUES (49, 28, 'https://m.media-amazon.com/images/I/71H5g-z1z-L._SL1500_.jpg', 4);
INSERT INTO public.product_images VALUES (50, 29, 'https://m.media-amazon.com/images/I/51UXXTQlWFL._SL1200_.jpg', 1);
INSERT INTO public.product_images VALUES (51, 29, 'https://m.media-amazon.com/images/I/61CEmOndu7L._SL1080_.jpg', 2);
INSERT INTO public.product_images VALUES (52, 29, 'https://m.media-amazon.com/images/I/51YXiW6pQoL._SL1080_.jpg', 3);
INSERT INTO public.product_images VALUES (53, 29, 'https://m.media-amazon.com/images/I/619r8v60jrL._SL1080_.jpg', 4);
INSERT INTO public.product_images VALUES (54, 30, 'https://m.media-amazon.com/images/I/61r4E88gZGL._SX679_.jpg', 1);
INSERT INTO public.product_images VALUES (55, 30, 'https://m.media-amazon.com/images/I/71tzlKc28OL._SX679_.jpg', 2);
INSERT INTO public.product_images VALUES (56, 30, 'https://m.media-amazon.com/images/I/61G5nnllEDL._SX679_.jpg', 3);
INSERT INTO public.product_images VALUES (57, 30, 'https://m.media-amazon.com/images/I/71MWRfmytfL._SX679_.jpg', 4);
INSERT INTO public.product_images VALUES (148, 14, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/d/x/x/-original-imah8pdgdzuf73yf.jpeg?q=90', 4);
INSERT INTO public.product_images VALUES (149, 15, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/v/7/a/-original-imah8pdgzhyfdveh.jpeg?q=90', 2);
INSERT INTO public.product_images VALUES (150, 15, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/l/p/n/-original-imah8pdgxbfqzkfb.jpeg?q=90', 1);
INSERT INTO public.product_images VALUES (151, 15, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/5/8/1/-original-imah8pdgckhgdudh.jpeg?q=90', 3);
INSERT INTO public.product_images VALUES (152, 15, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/d/l/l/-original-imah8wffnmejdzyy.jpeg?q=90', 4);
INSERT INTO public.product_images VALUES (58, 31, 'https://media.istockphoto.com/id/1412240771/photo/headphones-on-white-background.jpg?s=612x612&w=0&k=20&c=DwpnlOcMzclX8zJDKOMSqcXdc1E7gyGYgfX5Xr753aQ=', 1);
INSERT INTO public.product_images VALUES (59, 31, 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?fm=jpg&q=60&w=3000&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8Mnx8ZWFycGhvbmVzfGVufDB8fDB8fHww', 2);
INSERT INTO public.product_images VALUES (60, 31, 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?fm=jpg&q=60&w=3000&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8Mnx8ZWFycGhvbmVzfGVufDB8fDB8fHww', 3);
INSERT INTO public.product_images VALUES (73, 35, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/computer/z/w/o/-original-imahh9yhdhdefcmv.jpeg?q=90', 2);
INSERT INTO public.product_images VALUES (74, 35, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/computer/l/p/a/-original-imahh9yhnhwphsfy.jpeg?q=90', 3);
INSERT INTO public.product_images VALUES (75, 35, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/computer/k/i/1/-original-imahh9yhxfymhgpg.jpeg?q=90', 1);
INSERT INTO public.product_images VALUES (82, 4, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/7/c/o/-original-imahggesfx5yqphe.jpeg?q=90', 2);
INSERT INTO public.product_images VALUES (81, 4, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/v/2/w/-original-imahggesubmyd2ht.jpeg?q=90', 1);
INSERT INTO public.product_images VALUES (79, 4, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/r/z/s/-original-imahfvx3gkzzpjud.jpeg?q=90', 4);
INSERT INTO public.product_images VALUES (80, 4, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/g/r/v/-original-imahggeshyzhu9ue.jpeg?q=90', 3);
INSERT INTO public.product_images VALUES (83, 5, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/f/9/0/-original-imahggex9mdtt4se.jpeg?q=90', 1);
INSERT INTO public.product_images VALUES (84, 5, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/b/i/h/-original-imahggexyhuj9exm.jpeg?q=90', 2);
INSERT INTO public.product_images VALUES (85, 5, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/d/o/r/-original-imahfvx3ywpmyexy.jpeg?q=90', 3);
INSERT INTO public.product_images VALUES (86, 5, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/r/z/s/-original-imahfvx3gkzzpjud.jpeg?q=90', 4);
INSERT INTO public.product_images VALUES (93, 7, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/k/l/l/-original-imagtc5fz9spysyk.jpeg?q=90', 1);
INSERT INTO public.product_images VALUES (94, 7, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/u/m/3/-original-imagtc5ffhbausfy.jpeg?q=90', 2);
INSERT INTO public.product_images VALUES (95, 7, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/i/h/z/-original-imagtc5fbxefnjtj.jpeg?q=90', 3);
INSERT INTO public.product_images VALUES (96, 6, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/h/d/9/-original-imagtc2qzgnnuhxh.jpeg?q=90', 1);
INSERT INTO public.product_images VALUES (97, 6, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/a/v/k/-original-imagtc5fx9jzazdy.jpeg?q=90', 3);
INSERT INTO public.product_images VALUES (98, 6, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/i/h/z/-original-imagtc5fbxefnjtj.jpeg?q=90', 2);
INSERT INTO public.product_images VALUES (111, 32, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/headphone/v/d/g/-original-imahgr295uvptwq7.jpeg?q=90', 1);
INSERT INTO public.product_images VALUES (112, 32, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/headphone/4/b/0/-original-imahgr29vsg2fy78.jpeg?q=90', 4);
INSERT INTO public.product_images VALUES (113, 32, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/headphone/m/9/r/-original-imahgr29snkwgqqn.jpeg?q=90', 3);
INSERT INTO public.product_images VALUES (114, 32, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/headphone/w/q/w/-original-imahgr296huaxwty.jpeg?q=90', 2);
INSERT INTO public.product_images VALUES (115, 33, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/headphone/g/j/k/-original-imahgr29hggpahcg.jpeg?q=90', 2);
INSERT INTO public.product_images VALUES (116, 33, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/headphone/o/g/7/-original-imahgr29hqgfsmww.jpeg?q=90', 1);
INSERT INTO public.product_images VALUES (117, 33, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/headphone/z/k/w/-original-imahgr29wgeuhwkq.jpeg?q=90', 4);
INSERT INTO public.product_images VALUES (118, 33, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/headphone/u/9/s/-original-imahgr29rxdzhpxb.jpeg?q=90', 3);
INSERT INTO public.product_images VALUES (120, 34, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/headphone/d/5/v/-original-imahgr29e7fzcfgn.jpeg?q=90', 1);
INSERT INTO public.product_images VALUES (119, 34, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/headphone/8/4/z/-original-imahgr29zsmk4ygx.jpeg?q=90', 2);
INSERT INTO public.product_images VALUES (121, 34, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/headphone/e/x/k/-original-imahgr29ej3dpbqh.jpeg?q=90', 3);
INSERT INTO public.product_images VALUES (122, 34, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/headphone/i/r/p/-original-imahgr29qamppnkh.jpeg?q=90', 4);
INSERT INTO public.product_images VALUES (127, 2, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/computer/b/h/u/-original-imahzxav6vf3fd2k.jpeg?q=90', 3);
INSERT INTO public.product_images VALUES (126, 2, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/computer/u/s/c/-original-imahzxaverfywwjh.jpeg?q=90', 2);
INSERT INTO public.product_images VALUES (128, 2, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/computer/p/a/6/-original-imahzxavzbnxjhxp.jpeg?q=90', 1);
INSERT INTO public.product_images VALUES (129, 24, 'https://m.media-amazon.com/images/I/61wNsoxIa5L._SL1500_.jpg', 1);
INSERT INTO public.product_images VALUES (130, 24, 'https://m.media-amazon.com/images/I/61B7KwmFSPL._SL1500_.jpg', 2);
INSERT INTO public.product_images VALUES (131, 24, 'https://m.media-amazon.com/images/I/61i1jm-RdZL._SL1500_.jpg', 3);
INSERT INTO public.product_images VALUES (133, 10, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/j/b/f/-enriched-transparent-original-imahhyzzqbrxvvhx.png?q=90', 1);
INSERT INTO public.product_images VALUES (135, 10, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/v/z/z/-original-imahhyzzmzt4yzpk.jpeg?q=90', 2);
INSERT INTO public.product_images VALUES (134, 10, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/g/u/v/-original-imahhyzzyavvh8as.jpeg?q=90', 3);
INSERT INTO public.product_images VALUES (136, 10, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/z/c/p/-original-imahhyzzrgnbybxz.jpeg?q=90', 4);
INSERT INTO public.product_images VALUES (137, 11, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/1/0/q/-original-imahhyzhqdzy3udx.jpeg?q=90', 2);
INSERT INTO public.product_images VALUES (139, 11, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/9/1/p/-original-imahhyzhkz6nh5k8.jpeg?q=90', 3);
INSERT INTO public.product_images VALUES (138, 11, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/c/0/f/-original-imahhyzhvrav62ff.jpeg?q=90', 1);
INSERT INTO public.product_images VALUES (140, 11, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/f/i/j/-original-imahhyzhhzkyqhzw.jpeg?q=90', 4);
INSERT INTO public.product_images VALUES (141, 8, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/d/i/s/-original-imah8pdgwdu5b2hz.jpeg?q=90', 1);
INSERT INTO public.product_images VALUES (142, 8, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/c/4/o/-original-imah8pdgzr3tqyhm.jpeg?q=90', 3);
INSERT INTO public.product_images VALUES (143, 8, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/m/p/s/-original-imah8pdgc6vduxqv.jpeg?q=90', 4);
INSERT INTO public.product_images VALUES (144, 8, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/2/b/8/-original-imah8pdgvxdznyes.jpeg?q=90', 2);
INSERT INTO public.product_images VALUES (145, 14, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/u/l/v/-original-imah8pdgpjgyzhpx.jpeg?q=90', 3);
INSERT INTO public.product_images VALUES (146, 14, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/c/4/o/-original-imah8pdgzr3tqyhm.jpeg?q=90', 2);
INSERT INTO public.product_images VALUES (147, 14, 'https://rukminim2.flixcart.com/image/3024/3024/xif0q/mobile/j/e/r/-original-imah8pdgedd5whgs.jpeg?q=90', 1);


--
-- TOC entry 4053 (class 0 OID 18881)
-- Dependencies: 248
-- Data for Name: product_variants; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.product_variants VALUES (4, 2, 3, 74900, true, true, '{"RAM": "8GB", "Color": "White Titanium", "Display": "6.3-inch Super Retina XDR OLED, ProMotion 120 Hz", "Storage": "128 GB", "Processor": "Apple A18 Pro", "Rear Cameras": "48 MP Fusion + 48 MP Ultra Wide + 12 MP Telephoto (5× Optical Zoom)"}', 0);
INSERT INTO public.product_variants VALUES (5, 2, 6, 80900, false, true, '{"RAM": "8GB", "Color": "Black Titanium", "Display": "6.3-inch Super Retina XDR OLED, ProMotion 120 Hz", "Storage": "256 GB", "Processor": "Apple A18 Pro", "Front Camera": "12 MP TrueDepth", "Rear Cameras": "48 MP Fusion + 48 MP Ultra Wide + 12 MP Telephoto (5× Optical Zoom)", "Operating System": "iOS 18"}', 0);
INSERT INTO public.product_variants VALUES (20, 9, 10, 5000.00, false, true, '{"ANC": "Yes", "Color": "White", "Charging": "USB-C", "Battery Life": "30 Hours", "Water Resistance": "IP54"}', 0);
INSERT INTO public.product_variants VALUES (17, 8, 5, 2999.00, false, true, '{"color": "White", "Weight": "250 g", "Bluetooth": "5.2", "Battery Life": "30 Hours", "Noise Cancellation": "Yes"}', 0);
INSERT INTO public.product_variants VALUES (18, 8, 5, 2990.00, false, true, '{"color": "Black", "Weight": "250 g", "Bluetooth": "5.2", "Battery Life": "20 Hours", "Noise Cancellation": "Yes"}', 0);
INSERT INTO public.product_variants VALUES (32, 16, 10, 20000, false, true, '{"color": "Black", "Microphones": "8 microphones with AI-based noise reduction", "Battery Life": "Up to 30 hours (ANC On), Up to 40 hours (ANC Off)", "Connectivity": "Bluetooth 5.2, USB-C", "Product Type": "Over-Ear Wireless Headphones", "Audio Drivers": "30 mm Dynamic Drivers", "Charging Time": "Approximately 3.5 hours", "Wireless Range": "Up to 10 meters (33 ft)", "Voice Assistant": "Google Assistant, Amazon Alexa, Siri (via device)", "Noise Cancellation": "Industry-leading Active Noise Cancellation (ANC)"}', 0);
INSERT INTO public.product_variants VALUES (2, 1, 65, 174900, true, true, '{"RAM": "16GB", "Color": "Silver", "Storage": "1TB SSD"}', 0);
INSERT INTO public.product_variants VALUES (21, 9, 6, 5000.00, true, true, '{"ANC": "Yes", "Color": "Black", "Charging": "USB-C", "Battery Life": "30 Hours", "Water Resistance": "IP54"}', 0);
INSERT INTO public.product_variants VALUES (12, 5, 10, 80000.00, false, false, '{"RAM": "16 GB", "Color": "Black", "Storage": "256 GB"}', 0);
INSERT INTO public.product_variants VALUES (27, 12, 30, 5000.00, true, true, '{"Color": "Blue", "Output": "30 W", "Battery": "12 Hours", "Bluetooth": "5.1", "Water Resistance": "IP67"}', 0);
INSERT INTO public.product_variants VALUES (31, 15, 100, 2000, true, false, '{"key2": "value2", "key3": "value", "color": "black"}', 0);
INSERT INTO public.product_variants VALUES (30, 14, 2, 65000.00, true, true, '{"GPU": "NVIDIA GeForce RTX 5070", "DLSS": "DLSS 4", "Brand": "MSI", "Length": "338 mm", "Memory": "12 GB GDDR7", "Cooling": "Triple Fan", "CUDA Cores": "6144", "Memory Bus": "192-bit", "Boost Clock": "2610 MHz", "Ray Tracing": "4th Generation", "RGB Lighting": "Yes", "Display Outputs": "3 x DisplayPort 2.1b, 1 x HDMI 2.1b", "Recommended PSU": "650 W", "Power Consumption": "250 W"}', 0);
INSERT INTO public.product_variants VALUES (7, 3, 9, 90000, true, true, '{"RAM": "16 GB", "Color": "Blue", "Storage": "256 GB"}', 0);
INSERT INTO public.product_variants VALUES (25, 11, 15, 5000.00, false, false, '{"Color": "Black", "Output": "30 W", "Battery": "12 Hours", "Bluetooth": "5.1", "Water Resistance": "IP67"}', 0);
INSERT INTO public.product_variants VALUES (35, 1, 15, 120000, false, true, '{"color": "Pink Blush", "Battery": "72 Wh", "Display": "14.2-inch Liquid Retina XDR (3024 × 1964)", "Storage": "512 GB SSD", "Processor": "Apple M3 Pro (11-core CPU, 14-core GPU)", "Architecture": "ARM64 (Apple Silicon)", "Memory (RAM)": "18 GB Unified Memory", "Operating System": "macOS Sequoia 15.6"}', 0);
INSERT INTO public.product_variants VALUES (1, 1, 5, 200000.00, false, false, '{"RAM": "8GB", "Color": "Black", "Storage": "1TB SSD"}', 0);
INSERT INTO public.product_variants VALUES (19, 8, 2, 2999.00, true, true, '{"color": "Blue", "Weight": "250 g", "Bluetooth": "5.2", "Battery Life": "30 Hours", "Noise Cancellation": "Yes"}', 0);
INSERT INTO public.product_variants VALUES (22, 10, 112, 500.00, false, true, '{"DPI": "8000", "Color": "Graphite", "Battery": "70 Days", "Buttons": "7", "Connectivity": "Bluetooth + USB Receiver"}', 0);
INSERT INTO public.product_variants VALUES (28, 13, 29, 1000.00, false, true, '{"Color": "Black", "Layout": "TKL (87 Keys)", "Weight": "990 g", "Battery": "4000 mAh", "Keycaps": "Double-shot ABS", "Backlight": "White LED", "Switch Type": "Gateron Red", "Connectivity": "Bluetooth 5.1 / USB-C", "Compatible OS": "Windows, macOS, Linux", "Hot Swappable": "Yes"}', 0);
INSERT INTO public.product_variants VALUES (3, 1, 70, 194900, true, false, '{"RAM": "8GB", "Color": "Midnight", "Storage": "1TB SSD"}', 0);
INSERT INTO public.product_variants VALUES (6, 3, 8, 80000, false, true, '{"RAM": "8GB", "Color": "Black", "Display": "6.1-inch Super Retina XDR OLED, ProMotion 120 Hz", "Storage": "128 GB", "Front Camera": "12 MP TrueDepth", "Rear Cameras": "48 MP Main + 12 MP Ultra Wide + 12 MP Telephoto (3× Optical Zoom)"}', 0);
INSERT INTO public.product_variants VALUES (29, 13, 29, 1000.00, true, true, '{"Color": "White", "Layout": "TKL (87 Keys)", "Weight": "990 g", "Battery": "4000 mAh", "Keycaps": "Double-shot ABS", "Backlight": "White LED", "Switch Type": "Gateron Red", "Connectivity": "Bluetooth 5.1 / USB-C", "Compatible OS": "Windows, macOS, Linux", "Hot Swappable": "Yes"}', 0);
INSERT INTO public.product_variants VALUES (26, 11, 29, 5000.00, true, false, '{"Color": "Blue", "Output": "30 W", "Battery": "12 Hours", "Bluetooth": "5.1", "Water Resistance": "IP67"}', 0);
INSERT INTO public.product_variants VALUES (16, 7, 5, 90000.00, true, true, '{"Color": "Black", "Resolution": "4K UHD", "Screen Size": "55-inch", "Display Type": "QLED", "Operating System": "Tizen OS"}', 0);
INSERT INTO public.product_variants VALUES (15, 4, 10, 80000, false, true, '{"RAM": "16 GB", "Color": "Mint", "Storage": "256 GB"}', 0);
INSERT INTO public.product_variants VALUES (33, 16, 14, 23000, true, true, '{"color": "White"}', 0);
INSERT INTO public.product_variants VALUES (34, 16, 99, 25000, false, false, '{"color": "Smoky Pink", "Weight": "approx. 250g", "Foldable": "No (Swivel earcups only)", "Connectivity": "Bluetooth 5.2, USB-C", "Wireless Range": "Up to 10 meters (33 ft)", "Noise Cancellation": "Industry-leading Active Noise Cancellation (ANC)"}', 0);
INSERT INTO public.product_variants VALUES (24, 11, 14, 60000, true, true, '{"WiFi": "Wi-Fi 6", "Color": "White", "Edition": "Disc", "Storage": "1 TB SSD", "Resolution": "4K"}', 0);
INSERT INTO public.product_variants VALUES (23, 10, 12, 550.00, true, true, '{"DPI": "9000", "Color": "white", "Battery": "80 Days", "Buttons": "7", "Connectivity": "Bluetooth + USB Receiver"}', 0);
INSERT INTO public.product_variants VALUES (11, 5, 10, 80000, false, true, '{"RAM": "16 GB", "Color": "White", "Storage": "256 GB"}', 0);
INSERT INTO public.product_variants VALUES (13, 5, 10, 80000.00, false, false, '{"RAM": "16 GB", "Color": "Black", "Storage": "256 GB"}', 0);
INSERT INTO public.product_variants VALUES (9, 5, 10, 90000.00, false, false, '{"RAM": "16 GB", "Color": "Black", "Storage": "256 GB"}', 0);
INSERT INTO public.product_variants VALUES (10, 5, 10, 80000, true, true, '{"RAM": "16 GB", "Color": "White", "Storage": "256 GB"}', 0);
INSERT INTO public.product_variants VALUES (8, 4, 3, 90000, true, true, '{"RAM": "16GB", "Color": "Silver Shadow", "Storage": "128GB"}', 0);
INSERT INTO public.product_variants VALUES (14, 4, 10, 80000, false, true, '{"RAM": "8 GB", "Color": "Navy", "Storage": "128 GB"}', 0);


--
-- TOC entry 4051 (class 0 OID 18817)
-- Dependencies: 244
-- Data for Name: products; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.products VALUES (8, 4, 8, 'Sony WH-1000XM5 Wireless Noise Cancelling Headphones', 'Premium wireless headphones featuring industry-leading noise cancellation, 30-hour battery life, multipoint Bluetooth connectivity, and Hi-Res audio support.', 'Active', '2026-06-28 22:10:21.163882', 0, 0);
INSERT INTO public.products VALUES (2, 1, 3, 'Apple iphone 16 pro', 'Experience next-level performance with the Apple iPhone 16 Pro. Powered by the A18 Pro chip, it combines a stunning 6.3-inch Super Retina XDR display, advanced 48MP camera system, titanium design, and all-day battery life to deliver a premium smartphone experience for work, creativity, and entertainment.', 'Active', '2026-06-08 01:12:40.794744', 0, 0);
INSERT INTO public.products VALUES (4, 4, 3, 'Samasung Galaxy S25', 'The Samsung Galaxy S25 is Samsung''s premium compact flagship smartphone. It features a 6.2-inch Dynamic AMOLED 2X display with a 120Hz refresh rate, the powerful Snapdragon 8 Elite processor, 12GB RAM, and a triple rear camera system (50MP + 12MP + 10MP). Running on Android 15 with One UI 7, it offers advanced Galaxy AI features, IP68 water resistance, wireless charging, and up to seven years of software updates, making it a high-performance device for everyday use and photography.', 'Active', '2026-06-15 08:01:55.063262', 0, 0);
INSERT INTO public.products VALUES (10, 5, 1, 'Logitech MX Master 3S', 'Premium ergonomic wireless mouse with MagSpeed scrolling and customizable buttons', 'Active', '2026-06-28 22:28:16.458831', 0, 0);
INSERT INTO public.products VALUES (6, 6, 3, 'Samsung Galaxy S27 pro', 'The Samsung Galaxy S26 is a premium flagship smartphone featuring a vibrant AMOLED display, advanced Galaxy AI capabilities, and a powerful next-generation processor. With its versatile camera system and long-term software support, it delivers a seamless experience for photography, productivity, and everyday use.', 'Draft', '2026-06-16 14:40:31.210917', 0, 0);
INSERT INTO public.products VALUES (5, 6, 3, 'Samsung Galaxy S26 pro', 'The Samsung Galaxy S26 is a premium flagship smartphone featuring a vibrant AMOLED display, advanced Galaxy AI capabilities, and a powerful next-generation processor. With its versatile camera system and long-term software support, it delivers a seamless experience for photography, productivity, and everyday use.', 'Active', '2026-06-16 13:52:28.030452', 0, 0);
INSERT INTO public.products VALUES (7, 7, 7, 'Sony Bravia XR X90L 55-inch Full Array LED TV', 'Powered by Cognitive Processor XR, delivering realistic contrast and smooth motion for cinematic entertainment.', 'Active', '2026-06-19 15:28:09.106111', 0, 0);
INSERT INTO public.products VALUES (11, 5, 1, 'Sony PlayStation 5 Slim', 'Premium ergonomic wireless mouse with MagSpeed scrolling and customizable buttonsNext-generation gaming console with ultra-fast SSD, ray tracing, and 4K gaming support.', 'Active', '2026-06-28 22:33:07.128554', 0, 0);
INSERT INTO public.products VALUES (12, 5, 8, 'JBL Flip 6 Portable Bluetooth Speaker', 'Portable waterproof Bluetooth speaker delivering powerful sound with up to 12 hours of battery life.', 'Active', '2026-06-28 22:44:15.759191', 0, 0);
INSERT INTO public.products VALUES (15, 1, 8, 'Pro Wireless Headphones', 'Good Earphones', 'Archived', '2026-06-29 01:24:59.186548', 0, 0);
INSERT INTO public.products VALUES (16, 1, 8, 'Sony WH-1000XM5 Wireless Noise Cancelling Headphones', 'Experience premium sound quality with the Sony WH-1000XM5 Wireless Noise Cancelling Headphones. Featuring industry-leading active noise cancellation, up to 30 hours of battery life, crystal-clear hands-free calling, and multipoint Bluetooth connectivity. Designed for comfort with soft-fit leather ear cushions, these headphones are perfect for travel, work, gaming, and everyday music listening. Includes USB-C fast charging and a carrying case.', 'Active', '2026-07-05 17:34:30.84908', 0, 0);
INSERT INTO public.products VALUES (13, 7, 1, 'KeyChron K8 Pro Wireless Mechanical Keyboard', 'A premium tenkeyless mechanical keyboard featuring hot-swappable switches, Bluetooth 5.1, RGB backlighting, and support for Windows and macOS. Designed for gaming, programming, and productivity', 'Active', '2026-06-28 22:52:31.615429', 4, 2);
INSERT INTO public.products VALUES (9, 4, 8, 'Apple AirPods Pro (2nd Generation)', 'True wireless earbuds with Active Noise Cancellation, Transparency Mode, Personalized Spatial Audio, and USB-C charging.', 'Active', '2026-06-28 22:14:28.979689', 3, 1);
INSERT INTO public.products VALUES (14, 7, 1, 'NVIDIA GeForce RTX 5070', 'A high-performance graphics card built for 1440p and entry-level 4K gaming, AI workloads, and content creation. Features advanced ray tracing, DLSS, and efficient cooling.', 'Active', '2026-06-28 22:58:27.993551', 5, 1);
INSERT INTO public.products VALUES (1, 1, 4, 'Updated Apple MacBook Air 13-inch M3', 'The Apple MacBook Air 13-inch with M3 chip delivers exceptional performance, all-day battery life, and a lightweight design. Features a Liquid Retina display, 16GB unified memory, 512GB SSD storage, and macOS for seamless workflow.', 'Active', '2026-06-08 00:20:39.285574', 3.8, 3);
INSERT INTO public.products VALUES (3, 1, 3, 'Apple iphone 15 pro', 'Experience next-level performance with the Apple iPhone 16 Pro. Powered by the A18 Pro chip, it combines a stunning 6.3-inch Super Retina XDR display, advanced 48MP camera system, titanium design, and all-day battery life to deliver a premium smartphone experience for work, creativity, and entertainment.', 'Active', '2026-06-15 00:34:38.870982', 4.5, 3);


--
-- TOC entry 4043 (class 0 OID 18722)
-- Dependencies: 234
-- Data for Name: user_addresses; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.user_addresses VALUES (2, 4, 'Karthik', '1231231234', 'Line1', '', '', 'Coimbatore', 'TamilNadu', '123123', 'India', 'Home');
INSERT INTO public.user_addresses VALUES (3, 4, 'Karthik', '1231231234', 'Chennai Line1', 'Chennai Line 2', '', 'Chennai', 'TamilNadu', '134123', 'India', 'Work');
INSERT INTO public.user_addresses VALUES (4, 4, 'User1', '3386901444', 'KCY, 7th Main Street', 'XyZ Colony, ABC', 'Near Bus Terminus', 'LFYZ', 'Tamil Nadu', '641664', 'India', 'Work');
INSERT INTO public.user_addresses VALUES (5, 16, 'User3', '3386901444', 'KCY, 7th Main Street', 'XyZ Colony, ABC', 'Near Bus Terminus', 'LFYZ', 'Tamil Nadu', '641664', 'India', 'Work');
INSERT INTO public.user_addresses VALUES (6, 16, 'Dharshini', '3386901444', 'KCY, 7th Main Street', 'XyZ Colony, ABC', 'Near Bus Terminus', 'LFYZ', 'Tamil Nadu', '641664', 'India', 'Work');
INSERT INTO public.user_addresses VALUES (7, 18, 'Dharshini', '3386901444', 'KCY, 7th Main Street', 'XyZ Colony, ABC', 'Near Bus Terminus', 'LFYZ', 'Tamil Nadu', '641664', 'India', 'Work');
INSERT INTO public.user_addresses VALUES (8, 18, 'Karthik ', '1231231234', 'T2, abc Apartment,', 'C.S Nagar, Edayarpalayam road,', 'Near Workshop stop', 'Coimbatore', 'Tamil Nadu', '641010', 'India', 'Home');
INSERT INTO public.user_addresses VALUES (9, 21, 'sample2', '1231231234', 'T2, Sree Daksha Ananya Apartment,', 'XYZ colony,', 'Near Apollo Hospital', 'Coimbatore', 'Tamil Nadu', '641041', 'India', 'Home');


--
-- TOC entry 4037 (class 0 OID 18654)
-- Dependencies: 226
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.users VALUES (1, 'dharshini@gmail.com', '$2a$11$P99dYk/FWv0TsqIJSFngWuY6eMPBr1UEwfo6ITs9jwkdSD189HOKC', 'dharshini', 'Customer', true, '2026-06-03 16:21:51.115667');
INSERT INTO public.users VALUES (3, 'karthik@gmail.com', '$2a$11$yg8JLJHjmx5b2tRvk3eB6O/oOe3cw8OD2uP/6XRtIZyq65u6QTcii', 'Karthik M', 'Customer', true, '2026-06-04 11:44:57.025248');
INSERT INTO public.users VALUES (5, 'dharshnew@gmail.com', '$2a$11$q0ixmz/8wVZG6m/JZ.itWOOnQfOBnow5ujMa.j9yrr7MomCCDdEw6', 'dharsh', 'Customer', true, '2026-06-04 12:35:13.855865');
INSERT INTO public.users VALUES (7, 'postman@gmail.com', '$2a$11$F1zbNLvVYttvkIzAEvqHjeA9DTxW3U9hZK22V3HcvPK5EV5gNN5YO', 'postman', 'Customer', true, '2026-06-04 14:43:05.082462');
INSERT INTO public.users VALUES (8, 'admin2@gmail.com', '$2a$11$iNvDEuo7n/wfCWbFLeQHreMLTqCCcriF/1FGnQA2CwMv9u5ofOZ/W', 'postman', 'Customer', true, '2026-06-04 14:43:29.791481');
INSERT INTO public.users VALUES (9, 'admin3@gmail.com', '$2a$11$VnIe6WkPFXmSZl.P8x8Nqel70nJ06wx5juV8S6DHgxTTUrl57efra', 'postman', 'Customer', true, '2026-06-04 15:37:21.227484');
INSERT INTO public.users VALUES (2, 'dharsh@gmail.com', '', 'dharsh', 'Customer', true, '2026-06-03 16:23:59.401413');
INSERT INTO public.users VALUES (10, 'vendor1@gmail.com', '$2a$11$hPSBZOCyj02YKF4c97X88.GD30iP2DiHKG97omUSZZ3FkMVNeaSEu', 'Vendor1', 'Vendor', true, '2026-06-05 11:35:56.887004');
INSERT INTO public.users VALUES (12, 'vendor3@gmail.com', '$2a$11$f66DC0NVs.z9Pogha.igkeXlBhSoemq7FTiJDYexGH.ZcarsZFGWq', 'QWERTY electronics', 'Vendor', true, '2026-06-15 00:20:07.003064');
INSERT INTO public.users VALUES (13, 'vendor4@gmail.com', '$2a$11$kPGUa4Dd8jI8EEVIfH3uVek5pAe8SER8zwHDntNZyzmOWTeWr71Ei', 'Shopsy electronics', 'Vendor', true, '2026-06-15 17:05:54.010841');
INSERT INTO public.users VALUES (14, 'vendor5@gmail.com', '$2a$11$/ShWreiQkx3BGb/P44pYm.6neM6wP0snoKM21174SRulPlWpStmOy', 'Vector electronics', 'Vendor', true, '2026-06-16 13:43:33.312315');
INSERT INTO public.users VALUES (15, 'vendor6@gmail.com', '$2a$11$NkfgW/pwFSM5xGNAXLWNceydosBa1oOeHiKj6mK18wtw8v7P2qA0O', 'AtoZ electronics', 'Customer', true, '2026-06-16 14:37:32.049656');
INSERT INTO public.users VALUES (4, 'user@gmail.com', '$2a$11$fkLCy/6EhGooqi3ZUS0znuAIEmPuuJyTqgcrXGgRnCZdTlV4TJi6.', 'sample user', 'Vendor', true, '2026-06-04 12:32:24.885732');
INSERT INTO public.users VALUES (16, 'user3@gmail.com', '$2a$11$2jhAs00RrBj8tbCGgbtoH.3A248LfC40.F2ozGj5uOoWaJgd3Obo6', 'Customer', 'Customer', true, '2026-06-16 15:15:50.832392');
INSERT INTO public.users VALUES (17, 'dharshinikarthik@gmail.com', '$2a$11$e1Y02eBQtWSuXDLmSw20bO4kgXWMbW7a.otaGjwryYXpZdfFSVb8i', 'Dharshini Karthik', 'Customer', true, '2026-06-19 15:06:27.773926');
INSERT INTO public.users VALUES (18, 'dharshinikarthik06@gmail.com', '$2a$11$hz62PFOgXXFCxKigI0TLD.E9EstrhGZ1/HHcdb9/27rGuXLCsQyc2', 'Dharshini K', 'Customer', true, '2026-06-19 15:07:03.130371');
INSERT INTO public.users VALUES (19, 'cors@gmail.org', '$2a$11$nz/K4lOaCeFO94XUM7k3Gu1.oVv/Kpyh/QxIHlLQyNpiLTz0y77bS', 'CORSCheck', 'Customer', true, '2026-06-21 20:57:51.919714');
INSERT INTO public.users VALUES (20, 'stacy@shopy.com', '$2a$11$0vpwjy/TbGRwUA1Vox.NdO9mbyFCSKKoAdYuhFp.MBZTBH6.wYEvq', 'Stacy John', 'Customer', true, '2026-06-21 21:06:56.247473');
INSERT INTO public.users VALUES (6, 'dharshini.k2022cce@sece.ac.in', '$2a$11$Ca7.j6r4JnedgyeqBMdSne4GRxWPXsajoSDqHW5NRFB0zDeFIXlnq', 'Super Admin', 'Admin', true, '2026-06-04 13:41:45.927885');
INSERT INTO public.users VALUES (11, 'vendor2@gmail.com', '$2a$11$YVfJtTppFMW9uNOBkMPruuZD3cW/OFR/ddb9ITvQ7HnxqMfl1DM8m', 'vendor2', 'Vendor', true, '2026-06-05 11:47:36.859382');
INSERT INTO public.users VALUES (21, 'sample@gmail.com', '$2a$11$wwKiNrLN1aaI//koVkRT5eXlJv4yuon6pDqlSs7aTOIRhieJifbMC', 'sample', 'Customer', true, '2026-07-07 07:43:14.162921');


--
-- TOC entry 4045 (class 0 OID 18744)
-- Dependencies: 236
-- Data for Name: vendors; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.vendors VALUES (1, 11, 'abc electronics', 'dharshinikarthik06@gmail.com', 'AASPK1231231234', 'AASPK12345', 'Verified Seller of Dell, Lenovo, Apple and HP Products. 1000+ Happy Customers!', 'Approved', '', '2026-06-29 01:17:27.20199', true);
INSERT INTO public.vendors VALUES (2, 10, 'xyz fashions', 'xyzfashion@gmail.in', 'X2J52JRRI3DBE2H', 'QN5SP5PKZR', 'Leading women''s appreal', 'Approved', NULL, '2026-06-05 14:17:54.262647', true);
INSERT INTO public.vendors VALUES (4, 12, 'qwerty electornics', 'qwertyelec@gmail.in', 'AAPPM1231231234', 'QWERTY1234', 'Leading Women''s fashion appreal stores', 'Approved', '', '2026-06-15 00:32:02.789963', true);
INSERT INTO public.vendors VALUES (5, 13, 'shopsy electornics', 'shopsyelec@gmail.in', 'AAPPM1231231243', 'QWERTY1243', 'Leading Electornics Seller', 'Approved', '', '2026-06-15 17:23:39.999038', true);
INSERT INTO public.vendors VALUES (6, 14, 'vector electornics', 'vecelec@gmail.in', 'AAPPM1231231235', 'QWERTY1244', 'Leading Electornics Seller', 'Approved', '', '2026-06-16 13:50:48.815237', true);
INSERT INTO public.vendors VALUES (7, 4, 'atoz electornics', 'atoz@gmail.in', 'AAPPM1231231236', 'QWERTY1245', 'Leading Electornics Seller', 'Approved', '', '2026-06-16 14:39:33.952449', true);
INSERT INTO public.vendors VALUES (8, 20, 'shopy world', 'fashion@shopy.com', '22QQRRIIOOPP990', 'ABCDE12345', 'Exclusive fashion store', 'Pending', '', NULL, true);


--
-- TOC entry 4059 (class 0 OID 19059)
-- Dependencies: 262
-- Data for Name: wishlist_items; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.wishlist_items VALUES (1, 1, 1, '2026-06-08 17:12:07.154681');
INSERT INTO public.wishlist_items VALUES (5, 2, 21, '2026-07-01 10:51:17.245245');
INSERT INTO public.wishlist_items VALUES (6, 2, 31, '2026-07-01 10:52:36.639303');
INSERT INTO public.wishlist_items VALUES (7, 3, 23, '2026-07-07 07:45:53.625393');
INSERT INTO public.wishlist_items VALUES (9, 2, 28, '2026-07-08 11:19:45.869011');
INSERT INTO public.wishlist_items VALUES (12, 2, 6, '2026-07-08 12:54:57.934512');


--
-- TOC entry 4047 (class 0 OID 18763)
-- Dependencies: 238
-- Data for Name: wishlists; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.wishlists VALUES (1, 4, false);
INSERT INTO public.wishlists VALUES (2, 18, false);
INSERT INTO public.wishlists VALUES (3, 21, false);




