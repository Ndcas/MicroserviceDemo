-- phpMyAdmin SQL Dump
-- version 5.2.3
-- https://www.phpmyadmin.net/
--
-- Máy chủ: mysql
-- Thời gian đã tạo: Th9 11, 2026 lúc 11:19 AM
-- Phiên bản máy phục vụ: 9.7.1
-- Phiên bản PHP: 8.3.32

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Cơ sở dữ liệu: `product_service`
--

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `brands`
--

CREATE TABLE `brands` (
  `id` int NOT NULL,
  `name` varchar(255) NOT NULL,
  `image` varchar(255) DEFAULT NULL,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Đang đổ dữ liệu cho bảng `brands`
--

INSERT INTO `brands` (`id`, `name`, `image`, `created_at`) VALUES
(1, 'Samsung', 'samsung.png', '2026-08-27 15:56:14'),
(2, 'iPhone', 'iphone.png', '2026-08-27 15:56:14'),
(3, 'Asus', 'asus.png', '2026-08-27 15:56:14'),
(4, 'HP', 'hp.png', '2026-08-27 15:56:14');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `brand_type`
--

CREATE TABLE `brand_type` (
  `product_type_id` int NOT NULL,
  `brand_id` int NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Đang đổ dữ liệu cho bảng `brand_type`
--

INSERT INTO `brand_type` (`product_type_id`, `brand_id`) VALUES
(1, 1),
(1, 2),
(2, 3),
(2, 4);

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `inbox_messages`
--

CREATE TABLE `inbox_messages` (
  `event_id` char(36) NOT NULL,
  `processed_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `outbox_messages`
--

CREATE TABLE `outbox_messages` (
  `id` int NOT NULL,
  `event_id` char(36) NOT NULL,
  `topic` varchar(255) NOT NULL,
  `payload` text NOT NULL,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `published_at` timestamp NULL DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `products`
--

CREATE TABLE `products` (
  `id` int NOT NULL,
  `product_type_id` int NOT NULL,
  `brand_id` int NOT NULL,
  `name` varchar(255) NOT NULL,
  `image` varchar(255) DEFAULT NULL,
  `price` decimal(10,2) NOT NULL,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `stocks` int NOT NULL,
  `reserved` int NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Đang đổ dữ liệu cho bảng `products`
--

INSERT INTO `products` (`id`, `product_type_id`, `brand_id`, `name`, `image`, `price`, `created_at`, `updated_at`, `stocks`, `reserved`) VALUES
(1, 1, 1, 'Samsung Galaxy S26 FE 5G', 's26fe.jpg', 18990000.00, '2026-08-27 15:56:14', '2026-09-11 11:19:32', 100, 0),
(2, 1, 1, 'Samsung Galaxy Z Fold8', 'zfold8.jpg', 44990000.00, '2026-08-27 15:56:14', '2026-09-11 11:19:32', 100, 0),
(3, 1, 1, 'Samsung Galaxy Z Fold8 Ultra', 'zfold8ultra.jpg', 50990000.00, '2026-08-27 15:56:14', '2026-09-11 11:19:32', 100, 0),
(4, 1, 1, 'Samsung Galaxy A37 5G', 'a375g.jpg', 8990000.00, '2026-08-27 15:56:14', '2026-09-11 11:19:32', 100, 0),
(5, 1, 1, 'Samsung Galaxy S 26 5G', 's26.jpg', 21990000.00, '2026-08-27 15:56:14', '2026-09-11 11:19:32', 100, 0),
(6, 1, 2, 'iPhone 17 Pro Max', '17promax.jpg', 34590000.00, '2026-08-27 15:56:14', '2026-09-11 11:19:32', 100, 0),
(7, 1, 2, 'iPhone 16 Plus', '16plus.jpg', 24990000.00, '2026-08-27 15:56:14', '2026-09-11 11:19:32', 100, 0),
(8, 1, 2, 'iPhone 17', '17.jpg', 24590000.00, '2026-08-27 15:56:14', '2026-09-11 11:19:32', 100, 0),
(9, 1, 2, 'iPhone 17 Pro', '17pro.jpg', 31990000.00, '2026-08-27 15:56:14', '2026-09-11 11:19:32', 100, 0),
(10, 1, 2, 'iPhone 15', '15.jpg', 18990000.00, '2026-08-27 15:56:14', '2026-09-11 11:19:32', 100, 0),
(11, 2, 3, 'Asus Vivobook S14', 's14.jpg', 20190000.00, '2026-08-27 15:56:14', '2026-09-11 11:19:32', 100, 0),
(12, 2, 3, 'Asus Vivobook 16', 'v16.jpg', 18590000.00, '2026-08-27 15:56:14', '2026-09-11 11:19:32', 100, 0),
(13, 2, 3, 'Asus Vivobook 15', 'v15.jpg', 18990000.00, '2026-08-27 15:56:14', '2026-09-11 11:19:32', 100, 0),
(14, 2, 3, 'Asus TUF Gaming', 'tuf.jpg', 25990000.00, '2026-08-27 15:56:14', '2026-09-11 11:19:32', 100, 0),
(15, 2, 3, 'Asus Vivobook Go 15', 'go15.jpg', 17590000.00, '2026-08-27 15:56:14', '2026-09-11 11:19:32', 100, 0),
(16, 2, 4, 'HP 15', 'hp15.jpg', 19890000.00, '2026-08-27 15:56:14', '2026-09-11 11:19:32', 100, 0),
(17, 2, 4, 'HP 245', 'hp245.jpg', 18490000.00, '2026-08-27 15:56:14', '2026-09-11 11:19:32', 100, 0),
(18, 2, 4, 'HP 240R', 'hp240r.jpg', 14990000.00, '2026-08-27 15:56:14', '2026-09-11 11:19:32', 100, 0),
(19, 2, 4, 'HP Gaming Victus 15', 'victus15.jpg', 24990000.00, '2026-08-27 15:56:14', '2026-09-11 11:19:32', 100, 0),
(20, 2, 4, 'HP Pavilion 16', 'pavilion16.jpg', 22390000.00, '2026-08-27 15:56:14', '2026-09-11 11:19:32', 100, 0);

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `product_types`
--

CREATE TABLE `product_types` (
  `id` int NOT NULL,
  `name` varchar(255) NOT NULL,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Đang đổ dữ liệu cho bảng `product_types`
--

INSERT INTO `product_types` (`id`, `name`, `created_at`) VALUES
(1, 'Điện thoại', '2026-08-27 15:56:14'),
(2, 'Laptop', '2026-08-27 15:56:14');

--
-- Chỉ mục cho các bảng đã đổ
--

--
-- Chỉ mục cho bảng `brands`
--
ALTER TABLE `brands`
  ADD PRIMARY KEY (`id`);

--
-- Chỉ mục cho bảng `brand_type`
--
ALTER TABLE `brand_type`
  ADD PRIMARY KEY (`product_type_id`,`brand_id`),
  ADD KEY `brand_id` (`brand_id`);

--
-- Chỉ mục cho bảng `inbox_messages`
--
ALTER TABLE `inbox_messages`
  ADD PRIMARY KEY (`event_id`);

--
-- Chỉ mục cho bảng `outbox_messages`
--
ALTER TABLE `outbox_messages`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `event_id` (`event_id`);

--
-- Chỉ mục cho bảng `products`
--
ALTER TABLE `products`
  ADD PRIMARY KEY (`id`),
  ADD KEY `product_type_id` (`product_type_id`),
  ADD KEY `brand_id` (`brand_id`);

--
-- Chỉ mục cho bảng `product_types`
--
ALTER TABLE `product_types`
  ADD PRIMARY KEY (`id`);

--
-- AUTO_INCREMENT cho các bảng đã đổ
--

--
-- AUTO_INCREMENT cho bảng `brands`
--
ALTER TABLE `brands`
  MODIFY `id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT cho bảng `outbox_messages`
--
ALTER TABLE `outbox_messages`
  MODIFY `id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT cho bảng `products`
--
ALTER TABLE `products`
  MODIFY `id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=21;

--
-- AUTO_INCREMENT cho bảng `product_types`
--
ALTER TABLE `product_types`
  MODIFY `id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- Ràng buộc đối với các bảng kết xuất
--

--
-- Ràng buộc cho bảng `brand_type`
--
ALTER TABLE `brand_type`
  ADD CONSTRAINT `brand_type_ibfk_1` FOREIGN KEY (`product_type_id`) REFERENCES `product_types` (`id`),
  ADD CONSTRAINT `brand_type_ibfk_2` FOREIGN KEY (`brand_id`) REFERENCES `brands` (`id`);

--
-- Ràng buộc cho bảng `products`
--
ALTER TABLE `products`
  ADD CONSTRAINT `products_ibfk_1` FOREIGN KEY (`product_type_id`) REFERENCES `product_types` (`id`),
  ADD CONSTRAINT `products_ibfk_2` FOREIGN KEY (`brand_id`) REFERENCES `brands` (`id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
