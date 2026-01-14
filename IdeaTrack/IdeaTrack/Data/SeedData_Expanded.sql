-- =====================================================
-- EXPANDED SEED DATA FOR IDEATRACKDB
-- Minimum 20 rows per table
-- Generated: 2026-01-06
-- =====================================================

-- =====================================================
-- 1. DEPARTMENTS (20 rows)
-- =====================================================
SET IDENTITY_INSERT Departments ON;
INSERT INTO Departments (Id, Name, Code) VALUES
(1, N'Khoa Công nghệ Thông tin', 'CNTT'),
(2, N'Khoa Kinh tế', 'KT'),
(3, N'Khoa Xây dựng', 'XD'),
(4, N'Khoa Điện - Điện tử', 'DĐT'),
(5, N'Khoa Cơ khí', 'CK'),
(6, N'Khoa Hóa học', 'HH'),
(7, N'Khoa Sinh học', 'SH'),
(8, N'Khoa Ngoại ngữ', 'NN'),
(9, N'Khoa Toán học', 'TH'),
(10, N'Khoa Vật lý', 'VL'),
(11, N'Khoa Y học', 'YH'),
(12, N'Khoa Dược học', 'DH'),
(13, N'Khoa Luật', 'L'),
(14, N'Khoa Tâm lý học', 'TLH'),
(15, N'Khoa Xã hội học', 'XHH'),
(16, N'Khoa Lịch sử', 'LS'),
(17, N'Khoa Địa lý', 'DL'),
(18, N'Khoa Môi trường', 'MT'),
(19, N'Khoa Nghệ thuật', 'NT'),
(20, N'Phòng KHCN', 'PKHCN');
SET IDENTITY_INSERT Departments OFF;

-- =====================================================
-- 2. ACADEMIC YEARS (20 rows)
-- =====================================================
SET IDENTITY_INSERT AcademicYears ON;
INSERT INTO AcademicYears (Id, Name, IsCurrent, CreatedAt) VALUES
(1, N'Năm học 2015-2016', 0, '2015-09-01'),
(2, N'Năm học 2016-2017', 0, '2016-09-01'),
(3, N'Năm học 2017-2018', 0, '2017-09-01'),
(4, N'Năm học 2018-2019', 0, '2018-09-01'),
(5, N'Năm học 2019-2020', 0, '2019-09-01'),
(6, N'Năm học 2020-2021', 0, '2020-09-01'),
(7, N'Năm học 2021-2022', 0, '2021-09-01'),
(8, N'Năm học 2022-2023', 0, '2022-09-01'),
(9, N'Năm học 2023-2024', 0, '2023-09-01'),
(10, N'Năm học 2024-2025', 0, '2024-09-01'),
(11, N'Năm học 2025-2026', 1, '2025-09-01'),
(12, N'Năm học 2026-2027', 0, '2026-09-01'),
(13, N'Năm học 2027-2028', 0, '2027-09-01'),
(14, N'Năm học 2028-2029', 0, '2028-09-01'),
(15, N'Năm học 2029-2030', 0, '2029-09-01'),
(16, N'Năm học 2030-2031', 0, '2030-09-01'),
(17, N'Năm học 2031-2032', 0, '2031-09-01'),
(18, N'Năm học 2032-2033', 0, '2032-09-01'),
(19, N'Năm học 2033-2034', 0, '2033-09-01'),
(20, N'Năm học 2034-2035', 0, '2034-09-01');
SET IDENTITY_INSERT AcademicYears OFF;

-- =====================================================
-- 3. INITIATIVE PERIODS (25 rows - some OPEN)
-- =====================================================
SET IDENTITY_INSERT InitiativePeriods ON;
INSERT INTO InitiativePeriods (Id, Name, StartDate, EndDate, IsActive, AcademicYearId, CreatedAt) VALUES
-- Old closed periods
(1, N'Đợt 1 - HK1 2020-2021', '2020-10-01', '2020-12-31', 0, 6, '2020-09-01'),
(2, N'Đợt 2 - HK2 2020-2021', '2021-02-01', '2021-04-30', 0, 6, '2021-01-01'),
(3, N'Đợt 1 - HK1 2021-2022', '2021-10-01', '2021-12-31', 0, 7, '2021-09-01'),
(4, N'Đợt 2 - HK2 2021-2022', '2022-02-01', '2022-04-30', 0, 7, '2022-01-01'),
(5, N'Đợt 1 - HK1 2022-2023', '2022-10-01', '2022-12-31', 0, 8, '2022-09-01'),
(6, N'Đợt 2 - HK2 2022-2023', '2023-02-01', '2023-04-30', 0, 8, '2023-01-01'),
(7, N'Đợt 1 - HK1 2023-2024', '2023-10-01', '2023-12-31', 0, 9, '2023-09-01'),
(8, N'Đợt 2 - HK2 2023-2024', '2024-02-01', '2024-04-30', 0, 9, '2024-01-01'),
(9, N'Đợt 1 - HK1 2024-2025', '2024-10-01', '2024-12-31', 0, 10, '2024-09-01'),
(10, N'Đợt 2 - HK2 2024-2025', '2025-02-01', '2025-04-30', 0, 10, '2025-01-01'),
-- CURRENTLY OPEN PERIODS (2025-2026)
(11, N'Đợt NCKH - HK1 2025-2026', '2025-09-01', '2026-03-31', 1, 11, '2025-09-01'),
(12, N'Đợt Sáng kiến - HK1 2025-2026', '2025-10-01', '2026-02-28', 1, 11, '2025-10-01'),
(13, N'Đợt Đổi mới sáng tạo', '2026-01-01', '2026-06-30', 1, 11, '2026-01-01'),
-- Future periods
(14, N'Đợt 2 - HK2 2025-2026', '2026-03-01', '2026-06-30', 0, 11, '2026-02-01'),
(15, N'Đợt 1 - HK1 2026-2027', '2026-09-01', '2026-12-31', 0, 12, '2026-08-01'),
(16, N'Đợt 2 - HK2 2026-2027', '2027-02-01', '2027-04-30', 0, 12, '2027-01-01'),
(17, N'Đợt Khởi nghiệp 2025', '2025-11-01', '2026-05-31', 0, 11, '2025-10-15'),
(18, N'Đợt Ứng dụng AI', '2025-12-01', '2026-06-30', 1, 11, '2025-11-15'),
(19, N'Đợt Cải tiến QT', '2026-01-15', '2026-07-15', 1, 11, '2026-01-10'),
(20, N'Đợt NCKH đặc biệt', '2025-08-01', '2026-08-31', 1, 11, '2025-07-15'),
(21, N'Đợt Sáng kiến trẻ', '2026-01-01', '2026-12-31', 1, 11, '2025-12-01'),
(22, N'Đợt Nghiên cứu liên ngành', '2025-06-01', '2025-12-31', 0, 10, '2025-05-01'),
(23, N'Đợt Công nghệ xanh', '2026-02-01', '2026-08-31', 1, 11, '2026-01-15'),
(24, N'Đợt Chuyển đổi số', '2025-09-15', '2026-03-15', 1, 11, '2025-09-01'),
(25, N'Đợt Hợp tác quốc tế', '2026-01-01', '2026-09-30', 1, 11, '2025-12-15');
SET IDENTITY_INSERT InitiativePeriods OFF;

-- =====================================================
-- 4. EVALUATION TEMPLATES (20 rows)
-- =====================================================
SET IDENTITY_INSERT EvaluationTemplates ON;
INSERT INTO EvaluationTemplates (Id, TemplateName, Type, IsActive, CreatedAt) VALUES
(1, N'Mẫu chấm NCKH cơ bản', 0, 1, '2020-01-01'),
(2, N'Mẫu chấm Sáng kiến cải tiến', 0, 1, '2020-01-01'),
(3, N'Mẫu chấm Đề tài khoa học', 0, 1, '2020-01-01'),
(4, N'Mẫu chấm Báo cáo nghiên cứu', 0, 1, '2021-01-01'),
(5, N'Mẫu chấm Phát minh sáng chế', 0, 1, '2021-01-01'),
(6, N'Mẫu chấm Ứng dụng CNTT', 0, 1, '2022-01-01'),
(7, N'Mẫu chấm Giải pháp kỹ thuật', 0, 1, '2022-01-01'),
(8, N'Mẫu chấm Đổi mới sáng tạo', 0, 1, '2023-01-01'),
(9, N'Mẫu chấm Khởi nghiệp', 0, 1, '2023-01-01'),
(10, N'Mẫu chấm Nghiên cứu ứng dụng', 0, 1, '2023-06-01'),
(11, N'Mẫu chấm Bài báo quốc tế', 0, 1, '2024-01-01'),
(12, N'Mẫu chấm Cải tiến quy trình', 0, 1, '2024-01-01'),
(13, N'Mẫu chấm Nghiên cứu môi trường', 0, 1, '2024-06-01'),
(14, N'Mẫu chấm Công nghệ AI', 0, 1, '2025-01-01'),
(15, N'Mẫu chấm Năng lượng tái tạo', 0, 1, '2025-01-01'),
(16, N'Mẫu chấm Y sinh học', 0, 1, '2025-06-01'),
(17, N'Mẫu chấm Hóa sinh', 0, 1, '2025-06-01'),
(18, N'Mẫu chấm Vật liệu mới', 0, 1, '2025-09-01'),
(19, N'Mẫu chấm IoT/Robotics', 0, 1, '2025-09-01'),
(20, N'Mẫu chấm Blockchain/Fintech', 0, 1, '2025-12-01');
SET IDENTITY_INSERT EvaluationTemplates OFF;

-- =====================================================
-- 5. BOARDS (20 rows)
-- =====================================================
SET IDENTITY_INSERT Boards ON;
INSERT INTO Boards (Id, BoardName, FiscalYear, IsActive, CreatedAt) VALUES
(1, N'Hội đồng NCKH Trường', '2025-2026', 1, '2025-09-01'),
(2, N'Hội đồng Sáng kiến CNTT', '2025-2026', 1, '2025-09-01'),
(3, N'Hội đồng Sáng kiến Kinh tế', '2025-2026', 1, '2025-09-01'),
(4, N'Hội đồng Sáng kiến Xây dựng', '2025-2026', 1, '2025-09-01'),
(5, N'Hội đồng Sáng kiến Điện tử', '2025-2026', 1, '2025-09-01'),
(6, N'Hội đồng Sáng kiến Y học', '2025-2026', 1, '2025-09-01'),
(7, N'Hội đồng Đổi mới sáng tạo', '2025-2026', 1, '2025-09-01'),
(8, N'Hội đồng Khởi nghiệp', '2025-2026', 1, '2025-09-01'),
(9, N'Hội đồng Nghiên cứu Môi trường', '2025-2026', 1, '2025-09-01'),
(10, N'Hội đồng Công nghệ AI', '2025-2026', 1, '2025-09-01'),
(11, N'Hội đồng NCKH 2024-2025', '2024-2025', 0, '2024-09-01'),
(12, N'Hội đồng Sáng kiến 2024-2025', '2024-2025', 0, '2024-09-01'),
(13, N'Hội đồng NCKH 2023-2024', '2023-2024', 0, '2023-09-01'),
(14, N'Hội đồng Sáng kiến 2023-2024', '2023-2024', 0, '2023-09-01'),
(15, N'Hội đồng Liên ngành', '2025-2026', 1, '2025-10-01'),
(16, N'Hội đồng Quốc tế', '2025-2026', 1, '2025-10-01'),
(17, N'Hội đồng Sinh học & Dược', '2025-2026', 1, '2025-11-01'),
(18, N'Hội đồng Vật lý & Vật liệu', '2025-2026', 1, '2025-11-01'),
(19, N'Hội đồng Hóa học', '2025-2026', 1, '2025-11-01'),
(20, N'Hội đồng Chuyển đổi số', '2025-2026', 1, '2025-12-01');
SET IDENTITY_INSERT Boards OFF;

-- =====================================================
-- 6. INITIATIVE CATEGORIES (40 rows - linked to Boards & Templates)
-- =====================================================
SET IDENTITY_INSERT InitiativeCategories ON;
INSERT INTO InitiativeCategories (Id, Name, Description, PeriodId, BoardId, TemplateId) VALUES
-- Period 11: Đợt NCKH - HK1 2025-2026
(1, N'Đề tài NCKH cấp trường', N'Nghiên cứu khoa học cấp trường', 11, 1, 3),
(2, N'Đề tài NCKH cấp khoa', N'Nghiên cứu khoa học cấp khoa', 11, 1, 3),
(3, N'Báo cáo nghiên cứu sinh', N'Báo cáo tiến độ NCS', 11, 1, 4),
-- Period 12: Đợt Sáng kiến - HK1 2025-2026
(4, N'Sáng kiến cải tiến quy trình', N'Cải tiến quy trình làm việc', 12, 7, 2),
(5, N'Sáng kiến tiết kiệm năng lượng', N'Giải pháp tiết kiệm năng lượng', 12, 9, 15),
(6, N'Sáng kiến ứng dụng CNTT', N'Ứng dụng công nghệ thông tin', 12, 2, 6),
-- Period 13: Đợt Đổi mới sáng tạo
(7, N'Đổi mới sáng tạo - CNTT', N'Đổi mới trong lĩnh vực CNTT', 13, 2, 8),
(8, N'Đổi mới sáng tạo - Kinh tế', N'Đổi mới trong lĩnh vực kinh tế', 13, 3, 8),
(9, N'Đổi mới sáng tạo - Kỹ thuật', N'Đổi mới trong lĩnh vực kỹ thuật', 13, 4, 8),
(10, N'Đổi mới sáng tạo - Y học', N'Đổi mới trong lĩnh vực y học', 13, 6, 8),
-- Period 18: Đợt Ứng dụng AI
(11, N'AI trong giáo dục', N'Ứng dụng AI trong giáo dục', 18, 10, 14),
(12, N'AI trong y tế', N'Ứng dụng AI trong y tế', 18, 10, 14),
(13, N'AI trong kinh doanh', N'Ứng dụng AI trong kinh doanh', 18, 10, 14),
(14, N'Machine Learning', N'Nghiên cứu Machine Learning', 18, 10, 14),
-- Period 19: Đợt Cải tiến QT
(15, N'Cải tiến quy trình hành chính', N'Cải tiến quy trình hành chính', 19, 7, 12),
(16, N'Cải tiến quy trình đào tạo', N'Cải tiến quy trình đào tạo', 19, 7, 12),
-- Period 20: Đợt NCKH đặc biệt
(17, N'NCKH liên ngành', N'Nghiên cứu liên ngành đặc biệt', 20, 15, 10),
(18, N'NCKH hợp tác quốc tế', N'Nghiên cứu hợp tác quốc tế', 20, 16, 11),
-- Period 21: Đợt Sáng kiến trẻ
(19, N'Sáng kiến sinh viên', N'Sáng kiến từ sinh viên', 21, 8, 9),
(20, N'Sáng kiến giảng viên trẻ', N'Sáng kiến từ GV dưới 35 tuổi', 21, 8, 9),
(21, N'Ý tưởng khởi nghiệp', N'Ý tưởng khởi nghiệp sáng tạo', 21, 8, 9),
-- Period 23: Đợt Công nghệ xanh
(22, N'Năng lượng tái tạo', N'Nghiên cứu năng lượng tái tạo', 23, 9, 15),
(23, N'Công nghệ xử lý rác thải', N'Công nghệ xử lý môi trường', 23, 9, 13),
(24, N'Vật liệu thân thiện môi trường', N'Vật liệu sinh học', 23, 18, 18),
-- Period 24: Đợt Chuyển đổi số
(25, N'Chuyển đổi số hành chính', N'CĐS trong hành chính', 24, 20, 6),
(26, N'Chuyển đổi số đào tạo', N'CĐS trong giảng dạy', 24, 20, 6),
(27, N'IoT & Smart Campus', N'Ứng dụng IoT', 24, 20, 19),
-- Period 25: Đợt Hợp tác quốc tế
(28, N'Hợp tác nghiên cứu châu Âu', N'Dự án hợp tác châu Âu', 25, 16, 11),
(29, N'Hợp tác nghiên cứu châu Á', N'Dự án hợp tác châu Á', 25, 16, 11),
(30, N'Hợp tác nghiên cứu Mỹ', N'Dự án hợp tác với Mỹ', 25, 16, 11),
-- Additional categories
(31, N'Y sinh học phân tử', N'Nghiên cứu y sinh học', 11, 17, 16),
(32, N'Dược liệu thiên nhiên', N'Nghiên cứu dược liệu', 11, 17, 16),
(33, N'Blockchain ứng dụng', N'Ứng dụng blockchain', 13, 20, 20),
(34, N'Fintech', N'Công nghệ tài chính', 13, 20, 20),
(35, N'Robotics', N'Nghiên cứu robot', 18, 5, 19),
(36, N'Vật liệu nano', N'Nghiên cứu vật liệu nano', 20, 18, 18),
(37, N'Hóa sinh công nghiệp', N'Hóa sinh ứng dụng CN', 20, 19, 17),
(38, N'Điện tử công suất', N'NC điện tử công suất', 12, 5, 7),
(39, N'Xây dựng thông minh', N'Smart Construction', 12, 4, 7),
(40, N'Kinh tế số', N'Digital Economy research', 13, 3, 8);
SET IDENTITY_INSERT InitiativeCategories OFF;

-- =====================================================
-- 7. EVALUATION CRITERIA (60 rows - 3 per template)
-- =====================================================
SET IDENTITY_INSERT EvaluationCriteria ON;
INSERT INTO EvaluationCriteria (Id, TemplateId, Name, Description, MaxPoints, [Order]) VALUES
-- Template 1: NCKH cơ bản
(1, 1, N'Tính mới và sáng tạo', N'Đánh giá tính mới của đề tài', 30, 1),
(2, 1, N'Phương pháp nghiên cứu', N'Đánh giá phương pháp NC', 30, 2),
(3, 1, N'Khả năng ứng dụng', N'Khả năng ứng dụng thực tiễn', 40, 3),
-- Template 2: Sáng kiến cải tiến  
(4, 2, N'Tính khả thi', N'Đánh giá tính khả thi', 35, 1),
(5, 2, N'Hiệu quả kinh tế', N'Đánh giá hiệu quả kinh tế', 35, 2),
(6, 2, N'Tính lan tỏa', N'Khả năng nhân rộng', 30, 3),
-- Template 3: Đề tài khoa học
(7, 3, N'Cơ sở lý luận', N'Nền tảng lý thuyết', 25, 1),
(8, 3, N'Kết quả nghiên cứu', N'Đánh giá kết quả', 40, 2),
(9, 3, N'Đóng góp khoa học', N'Đóng góp cho ngành', 35, 3),
-- Template 4-20 (similar pattern)
(10, 4, N'Cấu trúc báo cáo', N'Đánh giá cấu trúc', 30, 1),
(11, 4, N'Nội dung báo cáo', N'Đánh giá nội dung', 40, 2),
(12, 4, N'Trình bày', N'Đánh giá trình bày', 30, 3),
(13, 5, N'Tính sáng chế', N'Đánh giá tính sáng chế', 40, 1),
(14, 5, N'Tính ứng dụng', N'Khả năng ứng dụng', 30, 2),
(15, 5, N'Tính thương mại', N'Khả năng thương mại hóa', 30, 3),
(16, 6, N'Giải pháp công nghệ', N'Đánh giá giải pháp', 35, 1),
(17, 6, N'Tính bảo mật', N'Đánh giá bảo mật', 30, 2),
(18, 6, N'Hiệu năng', N'Đánh giá hiệu năng', 35, 3),
(19, 7, N'Tính kỹ thuật', N'Đánh giá kỹ thuật', 40, 1),
(20, 7, N'An toàn', N'Đánh giá an toàn', 30, 2),
(21, 7, N'Chi phí', N'Đánh giá chi phí', 30, 3),
(22, 8, N'Tính đột phá', N'Đánh giá tính đột phá', 40, 1),
(23, 8, N'Tác động xã hội', N'Tác động lên xã hội', 30, 2),
(24, 8, N'Khả năng triển khai', N'Khả năng triển khai', 30, 3),
(25, 9, N'Mô hình kinh doanh', N'Đánh giá business model', 35, 1),
(26, 9, N'Tiềm năng thị trường', N'Đánh giá thị trường', 35, 2),
(27, 9, N'Đội ngũ', N'Đánh giá năng lực đội ngũ', 30, 3),
(28, 10, N'Tính thực tiễn', N'Đánh giá tính thực tiễn', 40, 1),
(29, 10, N'Kết quả ứng dụng', N'Đánh giá kết quả', 35, 2),
(30, 10, N'Đóng góp ngành', N'Đóng góp cho ngành', 25, 3),
(31, 11, N'Impact Factor', N'Đánh giá IF tạp chí', 40, 1),
(32, 11, N'Chất lượng bài báo', N'Đánh giá chất lượng', 35, 2),
(33, 11, N'Đóng góp tác giả', N'Vai trò của tác giả', 25, 3),
(34, 12, N'Mức độ cải tiến', N'Đánh giá mức độ cải tiến', 40, 1),
(35, 12, N'Tiết kiệm thời gian', N'Đánh giá tiết kiệm TG', 30, 2),
(36, 12, N'Tiết kiệm chi phí', N'Đánh giá tiết kiệm CP', 30, 3),
(37, 13, N'Tác động môi trường', N'Đánh giá tác động MT', 40, 1),
(38, 13, N'Bền vững', N'Đánh giá tính bền vững', 35, 2),
(39, 13, N'Khả năng nhân rộng', N'Đánh giá nhân rộng', 25, 3),
(40, 14, N'Mô hình AI', N'Đánh giá mô hình', 35, 1),
(41, 14, N'Dữ liệu huấn luyện', N'Đánh giá dữ liệu', 30, 2),
(42, 14, N'Độ chính xác', N'Đánh giá accuracy', 35, 3),
(43, 15, N'Hiệu suất năng lượng', N'Đánh giá hiệu suất', 40, 1),
(44, 15, N'Chi phí lắp đặt', N'Đánh giá chi phí', 30, 2),
(45, 15, N'Tuổi thọ', N'Đánh giá tuổi thọ', 30, 3),
(46, 16, N'Tính y khoa', N'Đánh giá tính y khoa', 40, 1),
(47, 16, N'An toàn sinh học', N'Đánh giá an toàn', 35, 2),
(48, 16, N'Hiệu quả điều trị', N'Đánh giá hiệu quả', 25, 3),
(49, 17, N'Phản ứng hóa học', N'Đánh giá phản ứng', 35, 1),
(50, 17, N'Hiệu suất', N'Đánh giá hiệu suất', 35, 2),
(51, 17, N'Ứng dụng CN', N'Đánh giá ứng dụng', 30, 3),
(52, 18, N'Tính chất vật liệu', N'Đánh giá tính chất', 40, 1),
(53, 18, N'Quy trình sản xuất', N'Đánh giá quy trình', 30, 2),
(54, 18, N'Ứng dụng thực tế', N'Đánh giá ứng dụng', 30, 3),
(55, 19, N'Chức năng robot', N'Đánh giá chức năng', 35, 1),
(56, 19, N'Tự động hóa', N'Đánh giá tự động', 35, 2),
(57, 19, N'Chi phí sản xuất', N'Đánh giá chi phí', 30, 3),
(58, 20, N'Tính bảo mật', N'Đánh giá bảo mật BC', 40, 1),
(59, 20, N'Khả năng mở rộng', N'Đánh giá scalability', 30, 2),
(60, 20, N'Tốc độ xử lý', N'Đánh giá performance', 30, 3);
SET IDENTITY_INSERT EvaluationCriteria OFF;

PRINT N'Seed data expanded successfully! 20+ rows per table.'
