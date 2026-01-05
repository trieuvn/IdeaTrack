-- ========================================
-- IDEATRACK SAMPLE DATA SEED SCRIPT
-- Run this script after database migration
-- ========================================

USE [IdeaTrackDB]
GO

-- ========================================
-- 1. DEPARTMENTS (10 rows)
-- ========================================
SET IDENTITY_INSERT [dbo].[Departments] ON
INSERT INTO [dbo].[Departments] ([Id], [Name], [Code]) VALUES
(1, N'Khoa Công nghệ Thông tin', 'CNTT'),
(2, N'Khoa Điện - Điện tử', 'DT'),
(3, N'Khoa Cơ khí', 'CK'),
(4, N'Khoa Kinh tế', 'KT'),
(5, N'Khoa Ngoại ngữ', 'NN'),
(6, N'Khoa Khoa học Cơ bản', 'KHCB'),
(7, N'Phòng KHCN', 'KHCN'),
(8, N'Khoa Xây dựng', 'XD'),
(9, N'Khoa Môi trường', 'MT'),
(10, N'Ban Giám hiệu', 'BGH')
SET IDENTITY_INSERT [dbo].[Departments] OFF
GO

-- ========================================
-- 2. ROLES (6 rows)
-- ========================================
SET IDENTITY_INSERT [dbo].[AspNetRoles] ON
INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp], [Description]) VALUES
(1, 'Admin', 'ADMIN', NEWID(), N'Quản trị hệ thống'),
(2, 'SciTech', 'SCITECH', NEWID(), N'Phòng KHCN'),
(3, 'FacultyLeader', 'FACULTYLEADER', NEWID(), N'Lãnh đạo Khoa'),
(4, 'CouncilMember', 'COUNCILMEMBER', NEWID(), N'Thành viên Hội đồng'),
(5, 'Lecturer', 'LECTURER', NEWID(), N'Giảng viên'),
(6, 'Author', 'AUTHOR', NEWID(), N'Tác giả sáng kiến')
SET IDENTITY_INSERT [dbo].[AspNetRoles] OFF
GO

-- ========================================
-- 3. USERS (20 rows)
-- Password: "123456" - Hash: AQAAAAIAAYagAAAAEBs0rDxMzP0vlZB6v6gNlAzUmSPpN5ot3wl1T1FQJvqwJ9qLQQQQQQQQQQQQQQQQQQ==
-- ========================================
SET IDENTITY_INSERT [dbo].[AspNetUsers] ON
INSERT INTO [dbo].[AspNetUsers] ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [FullName], [DepartmentId], [EmployeeCode]) VALUES
-- Admin & SciTech
(1, 'admin', 'ADMIN', 'admin@uni.edu.vn', 'ADMIN@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000001', 0, 0, NULL, 0, 0, N'Quản trị viên', 7, 'AD001'),
(2, 'scitech1', 'SCITECH1', 'scitech1@uni.edu.vn', 'SCITECH1@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000002', 0, 0, NULL, 0, 0, N'Nguyễn Văn KHCN', 7, 'ST001'),
(3, 'scitech2', 'SCITECH2', 'scitech2@uni.edu.vn', 'SCITECH2@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000003', 0, 0, NULL, 0, 0, N'Trần Thị KHCN', 7, 'ST002'),

-- Faculty Leaders
(4, 'leader_cntt', 'LEADER_CNTT', 'leader.cntt@uni.edu.vn', 'LEADER.CNTT@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000004', 0, 0, NULL, 0, 0, N'PGS.TS Lê Văn An', 1, 'FL001'),
(5, 'leader_dt', 'LEADER_DT', 'leader.dt@uni.edu.vn', 'LEADER.DT@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000005', 0, 0, NULL, 0, 0, N'TS. Phạm Văn Bình', 2, 'FL002'),
(6, 'leader_ck', 'LEADER_CK', 'leader.ck@uni.edu.vn', 'LEADER.CK@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000006', 0, 0, NULL, 0, 0, N'ThS. Hoàng Văn Cường', 3, 'FL003'),

-- Council Members
(7, 'council1', 'COUNCIL1', 'council1@uni.edu.vn', 'COUNCIL1@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000007', 0, 0, NULL, 0, 0, N'GS.TS Nguyễn Hữu Đức', 10, 'CM001'),
(8, 'council2', 'COUNCIL2', 'council2@uni.edu.vn', 'COUNCIL2@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000008', 0, 0, NULL, 0, 0, N'PGS.TS Trần Mai Hoa', 1, 'CM002'),
(9, 'council3', 'COUNCIL3', 'council3@uni.edu.vn', 'COUNCIL3@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000009', 0, 0, NULL, 0, 0, N'TS. Lê Minh Giang', 2, 'CM003'),
(10, 'council4', 'COUNCIL4', 'council4@uni.edu.vn', 'COUNCIL4@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000010', 0, 0, NULL, 0, 0, N'TS. Phạm Thị Hằng', 4, 'CM004'),
(11, 'council5', 'COUNCIL5', 'council5@uni.edu.vn', 'COUNCIL5@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000011', 0, 0, NULL, 0, 0, N'ThS. Vũ Văn Khánh', 3, 'CM005'),

-- Lecturers/Authors
(12, 'author1', 'AUTHOR1', 'author1@uni.edu.vn', 'AUTHOR1@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000012', 0, 0, NULL, 0, 0, N'ThS. Nguyễn Văn Linh', 1, 'AU001'),
(13, 'author2', 'AUTHOR2', 'author2@uni.edu.vn', 'AUTHOR2@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000013', 0, 0, NULL, 0, 0, N'ThS. Trần Thị Mai', 1, 'AU002'),
(14, 'author3', 'AUTHOR3', 'author3@uni.edu.vn', 'AUTHOR3@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000014', 0, 0, NULL, 0, 0, N'TS. Lê Hoàng Nam', 2, 'AU003'),
(15, 'author4', 'AUTHOR4', 'author4@uni.edu.vn', 'AUTHOR4@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000015', 0, 0, NULL, 0, 0, N'ThS. Phạm Văn Oanh', 2, 'AU004'),
(16, 'author5', 'AUTHOR5', 'author5@uni.edu.vn', 'AUTHOR5@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000016', 0, 0, NULL, 0, 0, N'ThS. Hoàng Thị Phương', 3, 'AU005'),
(17, 'author6', 'AUTHOR6', 'author6@uni.edu.vn', 'AUTHOR6@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000017', 0, 0, NULL, 0, 0, N'TS. Nguyễn Văn Quân', 4, 'AU006'),
(18, 'author7', 'AUTHOR7', 'author7@uni.edu.vn', 'AUTHOR7@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000018', 0, 0, NULL, 0, 0, N'ThS. Trần Văn Sơn', 5, 'AU007'),
(19, 'author8', 'AUTHOR8', 'author8@uni.edu.vn', 'AUTHOR8@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000019', 0, 0, NULL, 0, 0, N'ThS. Lê Thị Trang', 6, 'AU008'),
(20, 'author9', 'AUTHOR9', 'author9@uni.edu.vn', 'AUTHOR9@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000020', 0, 0, NULL, 0, 0, N'TS. Vũ Minh Uyên', 1, 'AU009')
SET IDENTITY_INSERT [dbo].[AspNetUsers] OFF
GO

-- ========================================
-- 4. USER ROLES
-- ========================================
INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES
(1, 1), -- admin -> Admin
(2, 2), -- scitech1 -> SciTech
(3, 2), -- scitech2 -> SciTech
(4, 3), -- leader_cntt -> FacultyLeader
(5, 3), -- leader_dt -> FacultyLeader
(6, 3), -- leader_ck -> FacultyLeader
(7, 4), -- council1 -> CouncilMember
(8, 4), -- council2 -> CouncilMember
(9, 4), -- council3 -> CouncilMember
(10, 4), -- council4 -> CouncilMember
(11, 4), -- council5 -> CouncilMember
(12, 5), (12, 6), -- author1 -> Lecturer, Author
(13, 5), (13, 6), -- author2 -> Lecturer, Author
(14, 5), (14, 6), -- author3 -> Lecturer, Author
(15, 5), (15, 6), -- author4 -> Lecturer, Author
(16, 5), (16, 6), -- author5 -> Lecturer, Author
(17, 5), (17, 6), -- author6 -> Lecturer, Author
(18, 5), (18, 6), -- author7 -> Lecturer, Author
(19, 5), (19, 6), -- author8 -> Lecturer, Author
(20, 5), (20, 6)  -- author9 -> Lecturer, Author
GO

-- ========================================
-- 5. ACADEMIC YEARS (3 rows)
-- ========================================
SET IDENTITY_INSERT [dbo].[AcademicYears] ON
INSERT INTO [dbo].[AcademicYears] ([Id], [Name], [StartDate], [EndDate], [IsCurrent]) VALUES
(1, N'Năm học 2024-2025', '2024-09-01', '2025-08-31', 0),
(2, N'Năm học 2025-2026', '2025-09-01', '2026-08-31', 1),
(3, N'Năm học 2026-2027', '2026-09-01', '2027-08-31', 0)
SET IDENTITY_INSERT [dbo].[AcademicYears] OFF
GO

-- ========================================
-- 6. INITIATIVE PERIODS (4 rows)
-- ========================================
SET IDENTITY_INSERT [dbo].[InitiativePeriods] ON
INSERT INTO [dbo].[InitiativePeriods] ([Id], [Name], [Description], [StartDate], [EndDate], [IsActive], [AcademicYearId], [CreatedAt]) VALUES
(1, N'Đợt Sáng kiến HK1 2024-2025', N'Đợt sáng kiến học kỳ 1 năm học 2024-2025', '2024-09-01', '2024-12-31', 0, 1, '2024-08-15'),
(2, N'Đợt Sáng kiến HK2 2024-2025', N'Đợt sáng kiến học kỳ 2 năm học 2024-2025', '2025-01-01', '2025-06-30', 0, 1, '2024-12-15'),
(3, N'Đợt Sáng kiến HK1 2025-2026', N'Đợt sáng kiến học kỳ 1 năm học 2025-2026', '2025-09-01', '2025-12-31', 1, 2, '2025-08-15'),
(4, N'Đợt Sáng kiến HK2 2025-2026', N'Đợt sáng kiến học kỳ 2 năm học 2025-2026', '2026-01-01', '2026-06-30', 0, 2, '2025-12-15')
SET IDENTITY_INSERT [dbo].[InitiativePeriods] OFF
GO

-- ========================================
-- 7. EVALUATION TEMPLATES (3 rows)
-- ========================================
SET IDENTITY_INSERT [dbo].[EvaluationTemplates] ON
INSERT INTO [dbo].[EvaluationTemplates] ([Id], [TemplateName], [Description], [TemplateType], [MaxTotalScore], [IsActive]) VALUES
(1, N'Mẫu chấm Sáng kiến Kỹ thuật', N'Dùng cho các sáng kiến kỹ thuật, công nghệ', 1, 100, 1),
(2, N'Mẫu chấm Sáng kiến Quản lý', N'Dùng cho các sáng kiến quản lý, quy trình', 1, 100, 1),
(3, N'Mẫu sơ duyệt Khoa', N'Mẫu sơ duyệt cấp Khoa', 0, 100, 1)
SET IDENTITY_INSERT [dbo].[EvaluationTemplates] OFF
GO

-- ========================================
-- 8. EVALUATION CRITERIA (15 rows)
-- ========================================
SET IDENTITY_INSERT [dbo].[EvaluationCriteria] ON
-- Template 1: Kỹ thuật
INSERT INTO [dbo].[EvaluationCriteria] ([Id], [TemplateId], [CriteriaName], [Description], [MaxScore], [SortOrder]) VALUES
(1, 1, N'Tính mới', N'Mức độ mới mẻ, sáng tạo của giải pháp', 25, 1),
(2, 1, N'Tính khả thi', N'Khả năng áp dụng vào thực tế', 20, 2),
(3, 1, N'Hiệu quả kinh tế', N'Lợi ích kinh tế mang lại', 25, 3),
(4, 1, N'Khả năng nhân rộng', N'Có thể áp dụng ở nhiều nơi', 15, 4),
(5, 1, N'Chất lượng hồ sơ', N'Tính đầy đủ, rõ ràng của hồ sơ', 15, 5),

-- Template 2: Quản lý
(6, 2, N'Tính mới', N'Mức độ đổi mới trong quy trình', 20, 1),
(7, 2, N'Tính hiệu quả', N'Cải thiện hiệu suất công việc', 25, 2),
(8, 2, N'Tiết kiệm chi phí', N'Giảm chi phí hoạt động', 20, 3),
(9, 2, N'Dễ triển khai', N'Dễ dàng áp dụng', 20, 4),
(10, 2, N'Chất lượng hồ sơ', N'Tính đầy đủ, rõ ràng', 15, 5),

-- Template 3: Sơ duyệt
(11, 3, N'Đủ điều kiện nộp', N'Đáp ứng các yêu cầu cơ bản', 30, 1),
(12, 3, N'Tính khả thi sơ bộ', N'Đánh giá sơ bộ tính khả thi', 30, 2),
(13, 3, N'Hồ sơ đầy đủ', N'Có đầy đủ các biểu mẫu', 40, 3)
SET IDENTITY_INSERT [dbo].[EvaluationCriteria] OFF
GO

-- ========================================
-- 9. BOARDS (3 rows)
-- ========================================
SET IDENTITY_INSERT [dbo].[Boards] ON
INSERT INTO [dbo].[Boards] ([Id], [BoardName], [Description], [IsActive]) VALUES
(1, N'Hội đồng Sáng kiến CNTT', N'Chấm sáng kiến lĩnh vực CNTT', 1),
(2, N'Hội đồng Sáng kiến Kỹ thuật', N'Chấm sáng kiến lĩnh vực Kỹ thuật', 1),
(3, N'Hội đồng Sáng kiến Quản lý', N'Chấm sáng kiến lĩnh vực Quản lý', 1)
SET IDENTITY_INSERT [dbo].[Boards] OFF
GO

-- ========================================
-- 10. BOARD MEMBERS (10 rows)
-- ========================================
SET IDENTITY_INSERT [dbo].[BoardMembers] ON
INSERT INTO [dbo].[BoardMembers] ([Id], [BoardId], [UserId], [Role], [JoinDate]) VALUES
-- Board CNTT
(1, 1, 7, 0, '2025-01-01'),  -- council1 - Chairman
(2, 1, 8, 1, '2025-01-01'),  -- council2 - Member
(3, 1, 9, 1, '2025-01-01'),  -- council3 - Member
-- Board Kỹ thuật
(4, 2, 7, 0, '2025-01-01'),  -- council1 - Chairman
(5, 2, 10, 1, '2025-01-01'), -- council4 - Member
(6, 2, 11, 1, '2025-01-01'), -- council5 - Member
-- Board Quản lý
(7, 3, 8, 0, '2025-01-01'),  -- council2 - Chairman
(8, 3, 9, 1, '2025-01-01'),  -- council3 - Member
(9, 3, 10, 1, '2025-01-01'), -- council4 - Member
(10, 3, 11, 1, '2025-01-01') -- council5 - Member
SET IDENTITY_INSERT [dbo].[BoardMembers] OFF
GO

-- ========================================
-- 11. INITIATIVE CATEGORIES (8 rows)
-- ========================================
SET IDENTITY_INSERT [dbo].[InitiativeCategories] ON
INSERT INTO [dbo].[InitiativeCategories] ([Id], [PeriodId], [Name], [Description], [BoardId], [TemplateId]) VALUES
-- Period 3 (Active)
(1, 3, N'Sáng kiến Phần mềm', N'Các sáng kiến về phần mềm, ứng dụng', 1, 1),
(2, 3, N'Sáng kiến Phần cứng/IoT', N'Các sáng kiến về thiết bị, IoT', 2, 1),
(3, 3, N'Sáng kiến Tự động hóa', N'Các sáng kiến tự động hóa quy trình', 2, 1),
(4, 3, N'Sáng kiến Quản lý đào tạo', N'Cải tiến quy trình đào tạo', 3, 2),
(5, 3, N'Sáng kiến Quản lý hành chính', N'Cải tiến quy trình hành chính', 3, 2),
-- Period 1 (Closed)
(6, 1, N'Sáng kiến CNTT', N'Sáng kiến CNTT HK1', 1, 1),
(7, 1, N'Sáng kiến Kỹ thuật', N'Sáng kiến Kỹ thuật HK1', 2, 1),
(8, 1, N'Sáng kiến Quản lý', N'Sáng kiến Quản lý HK1', 3, 2)
SET IDENTITY_INSERT [dbo].[InitiativeCategories] OFF
GO

-- ========================================
-- 12. INITIATIVES (25 rows - various statuses)
-- ========================================
SET IDENTITY_INSERT [dbo].[Initiatives] ON
INSERT INTO [dbo].[Initiatives] ([Id], [InitiativeCode], [Title], [Description], [Solution], [Benefit], [Budget], [Status], [CreatorId], [DepartmentId], [CategoryId], [PeriodId], [CurrentRound], [CreatedAt], [SubmittedDate]) VALUES
-- Draft (3)
(1, 'SK2025-001', N'Hệ thống quản lý thư viện thông minh', N'Xây dựng hệ thống quản lý thư viện sử dụng AI', N'Sử dụng ML để recommend sách', N'Tăng hiệu quả 30%', 50000000, 0, 12, 1, 1, 3, 1, '2025-11-01', NULL),
(2, 'SK2025-002', N'Ứng dụng điểm danh bằng QR Code', N'Điểm danh sinh viên bằng QR code', N'Sinh viên quét QR để điểm danh', N'Tiết kiệm thời gian', 10000000, 0, 13, 1, 1, 3, 1, '2025-11-05', NULL),
(3, 'SK2025-003', N'Chatbot hỗ trợ sinh viên', N'Chatbot tư vấn tuyển sinh và học vụ', N'Sử dụng NLP xử lý câu hỏi', N'Giảm tải nhân sự', 30000000, 0, 14, 2, 2, 3, 1, '2025-11-10', NULL),

-- Pending (4)
(4, 'SK2025-004', N'Hệ thống IoT giám sát phòng học', N'Giám sát nhiệt độ, độ ẩm, ánh sáng phòng học', N'Lắp đặt cảm biến IoT', N'Tiết kiệm điện 20%', 80000000, 1, 15, 2, 2, 3, 1, '2025-10-15', '2025-10-20'),
(5, 'SK2025-005', N'Robot hướng dẫn viên triển lãm', N'Robot tự động hướng dẫn khách tham quan', N'Sử dụng ROS và AI', N'Thu hút khách tham quan', 150000000, 1, 16, 3, 3, 3, 1, '2025-10-18', '2025-10-25'),
(6, 'SK2025-006', N'Phần mềm quản lý đề tài NCKH', N'Quản lý đề tài từ đăng ký đến nghiệm thu', N'Xây dựng web app với .NET', N'Quản lý tập trung', 40000000, 1, 17, 4, 4, 3, 1, '2025-10-20', '2025-10-28'),
(7, 'SK2025-007', N'Quy trình xử lý hồ sơ điện tử', N'Số hóa quy trình xử lý hồ sơ sinh viên', N'Triển khai eOffice', N'Giảm thời gian 50%', 25000000, 1, 18, 5, 5, 3, 1, '2025-10-22', '2025-10-30'),

-- Faculty_Approved (3)
(8, 'SK2025-008', N'Hệ thống thi trực tuyến', N'Nền tảng thi trực tuyến có chống gian lận', N'Sử dụng proctoring AI', N'Linh hoạt tổ chức thi', 60000000, 2, 12, 1, 1, 3, 1, '2025-09-15', '2025-09-20'),
(9, 'SK2025-009', N'Máy in 3D giáo dục', N'Lắp ráp máy in 3D cho học tập', N'Thiết kế và lắp ráp tại trường', N'Phục vụ đào tạo', 45000000, 2, 14, 2, 2, 3, 1, '2025-09-18', '2025-09-25'),
(10, 'SK2025-010', N'Hệ thống tưới tự động', N'Tưới cây tự động trong khuôn viên', N'Sử dụng Arduino và cảm biến', N'Tiết kiệm nước 40%', 35000000, 2, 15, 2, 3, 3, 1, '2025-09-20', '2025-09-28'),

-- Evaluating (4)
(11, 'SK2025-011', N'Phần mềm chấm bài tự động', N'Chấm bài trắc nghiệm tự động bằng OMR', N'Sử dụng OpenCV xử lý ảnh', N'Tiết kiệm thời gian chấm', 20000000, 3, 13, 1, 1, 3, 1, '2025-09-01', '2025-09-05'),
(12, 'SK2025-012', N'Drone giám sát an ninh', N'Drone tuần tra khuôn viên trường', N'Lập trình bay tự động', N'Tăng cường an ninh', 120000000, 3, 16, 3, 3, 3, 1, '2025-09-05', '2025-09-10'),
(13, 'SK2025-013', N'App quản lý ký túc xá', N'Ứng dụng cho sinh viên ký túc xá', N'React Native mobile app', N'Quản lý tiện lợi', 25000000, 3, 19, 1, 1, 3, 1, '2025-09-08', '2025-09-12'),
(14, 'SK2025-014', N'Hệ thống lọc nước thông minh', N'Lọc nước uống thông minh', N'Kết hợp IoT giám sát chất lượng', N'Đảm bảo sức khỏe', 70000000, 3, 20, 1, 2, 3, 1, '2025-09-10', '2025-09-15'),

-- Pending_Final (3)
(15, 'SK2025-015', N'Phần mềm đăng ký môn học', N'Cải tiến hệ thống đăng ký môn học', N'Tối ưu thuật toán xếp lịch', N'Giảm tình trạng đụng lịch', 55000000, 6, 12, 1, 1, 3, 1, '2025-08-01', '2025-08-05'),
(16, 'SK2025-016', N'Xe điện tự hành trong trường', N'Xe điện chở người trong khuôn viên', N'Sử dụng LIDAR và camera', N'Di chuyển thuận tiện', 200000000, 6, 14, 2, 3, 3, 1, '2025-08-05', '2025-08-10'),
(17, 'SK2025-017', N'Hệ thống quản lý năng lượng', N'Tối ưu tiêu thụ điện năng', N'Lắp smart meter và AI phân tích', N'Giảm điện 25%', 90000000, 6, 17, 4, 4, 3, 1, '2025-08-08', '2025-08-12'),

-- Approved (5)
(18, 'SK2024-001', N'Phần mềm quản lý điểm', N'Quản lý điểm sinh viên online', N'Web app .NET Core + SQL', N'Tra cứu nhanh', 40000000, 7, 12, 1, 6, 1, 1, '2024-09-01', '2024-09-05'),
(19, 'SK2024-002', N'Hệ thống giám sát camera AI', N'Nhận dạng khuôn mặt và phát hiện bất thường', N'Sử dụng TensorFlow', N'Tăng an ninh', 100000000, 7, 13, 1, 6, 1, 1, '2024-09-10', '2024-09-15'),
(20, 'SK2024-003', N'Robot pha cà phê tự động', N'Robot barista trong căng tin', N'Arduino + mechanics', N'Phục vụ nhanh', 80000000, 7, 16, 3, 7, 1, 1, '2024-09-15', '2024-09-20'),
(21, 'SK2024-004', N'Hệ thống quản lý thực tập', N'Theo dõi sinh viên thực tập', N'Web portal kết nối DN', N'Quản lý hiệu quả', 30000000, 7, 17, 4, 8, 1, 1, '2024-09-20', '2024-09-25'),
(22, 'SK2024-005', N'App học tiếng Anh', N'Ứng dụng luyện phát âm', N'Speech recognition', N'Học mọi lúc mọi nơi', 35000000, 7, 18, 5, 8, 1, 1, '2024-09-25', '2024-09-30'),

-- Rejected (3)
(23, 'SK2024-006', N'Máy bay không người lái', N'Drone phun thuốc trừ sâu', N'Chế tạo drone nông nghiệp', N'Tiết kiệm nhân công', 500000000, 8, 15, 2, 7, 1, 1, '2024-10-01', '2024-10-05'),
(24, 'SK2024-007', N'Hệ thống VR đào tạo', N'Đào tạo thực hành bằng VR', N'Xây dựng môi trường VR', N'An toàn thực hành', 300000000, 8, 19, 1, 6, 1, 1, '2024-10-05', '2024-10-10'),
(25, 'SK2024-008', N'Phần mềm quản lý tài sản', N'Quản lý tài sản cố định', N'Web app', N'Kiểm kê dễ dàng', 25000000, 8, 20, 1, 8, 1, 1, '2024-10-10', '2024-10-15')
SET IDENTITY_INSERT [dbo].[Initiatives] OFF
GO

-- ========================================
-- 13. INITIATIVE AUTHORSHIPS (30 rows)
-- ========================================
SET IDENTITY_INSERT [dbo].[InitiativeAuthorships] ON
INSERT INTO [dbo].[InitiativeAuthorships] ([Id], [InitiativeId], [AuthorId], [ContributionPercent], [Role], [AddedAt]) VALUES
-- Primary authors
(1, 1, 12, 100, 0, '2025-11-01'),
(2, 2, 13, 100, 0, '2025-11-05'),
(3, 3, 14, 70, 0, '2025-11-10'),
(4, 3, 15, 30, 1, '2025-11-10'), -- Co-author
(5, 4, 15, 100, 0, '2025-10-15'),
(6, 5, 16, 60, 0, '2025-10-18'),
(7, 5, 17, 40, 1, '2025-10-18'), -- Co-author
(8, 6, 17, 100, 0, '2025-10-20'),
(9, 7, 18, 100, 0, '2025-10-22'),
(10, 8, 12, 80, 0, '2025-09-15'),
(11, 8, 13, 20, 1, '2025-09-15'), -- Co-author
(12, 9, 14, 100, 0, '2025-09-18'),
(13, 10, 15, 100, 0, '2025-09-20'),
(14, 11, 13, 100, 0, '2025-09-01'),
(15, 12, 16, 50, 0, '2025-09-05'),
(16, 12, 17, 50, 1, '2025-09-05'), -- Co-author
(17, 13, 19, 100, 0, '2025-09-08'),
(18, 14, 20, 100, 0, '2025-09-10'),
(19, 15, 12, 100, 0, '2025-08-01'),
(20, 16, 14, 60, 0, '2025-08-05'),
(21, 16, 15, 40, 1, '2025-08-05'), -- Co-author
(22, 17, 17, 100, 0, '2025-08-08'),
(23, 18, 12, 100, 0, '2024-09-01'),
(24, 19, 13, 100, 0, '2024-09-10'),
(25, 20, 16, 100, 0, '2024-09-15'),
(26, 21, 17, 100, 0, '2024-09-20'),
(27, 22, 18, 100, 0, '2024-09-25'),
(28, 23, 15, 100, 0, '2024-10-01'),
(29, 24, 19, 100, 0, '2024-10-05'),
(30, 25, 20, 100, 0, '2024-10-10')
SET IDENTITY_INSERT [dbo].[InitiativeAuthorships] OFF
GO

-- ========================================
-- 14. INITIATIVE ASSIGNMENTS (20 rows)
-- ========================================
SET IDENTITY_INSERT [dbo].[InitiativeAssignments] ON
INSERT INTO [dbo].[InitiativeAssignments] ([Id], [InitiativeId], [BoardId], [MemberId], [TemplateId], [RoundNumber], [AssignedDate], [DueDate], [StageName], [Status], [Decision], [ReviewComment], [DecisionDate]) VALUES
-- Evaluating initiatives (assigned to council members)
(1, 11, 1, 7, 1, 1, '2025-09-10', '2025-09-25', N'Vòng 1', 2, NULL, N'Đề tài hay', '2025-09-20'),
(2, 11, 1, 8, 1, 1, '2025-09-10', '2025-09-25', N'Vòng 1', 2, NULL, N'Cần bổ sung tài liệu', '2025-09-22'),
(3, 11, 1, 9, 1, 1, '2025-09-10', '2025-09-25', N'Vòng 1', 1, NULL, NULL, NULL),
(4, 12, 2, 7, 1, 1, '2025-09-15', '2025-09-30', N'Vòng 1', 1, NULL, NULL, NULL),
(5, 12, 2, 10, 1, 1, '2025-09-15', '2025-09-30', N'Vòng 1', 1, NULL, NULL, NULL),
(6, 13, 1, 8, 1, 1, '2025-09-18', '2025-10-03', N'Vòng 1', 0, NULL, NULL, NULL),
(7, 13, 1, 9, 1, 1, '2025-09-18', '2025-10-03', N'Vòng 1', 0, NULL, NULL, NULL),
(8, 14, 2, 10, 1, 1, '2025-09-20', '2025-10-05', N'Vòng 1', 0, NULL, NULL, NULL),
(9, 14, 2, 11, 1, 1, '2025-09-20', '2025-10-05', N'Vòng 1', 0, NULL, NULL, NULL),

-- Pending_Final (all assignments completed)
(10, 15, 1, 7, 1, 1, '2025-08-10', '2025-08-25', N'Vòng 1', 2, NULL, N'Rất tốt', '2025-08-20'),
(11, 15, 1, 8, 1, 1, '2025-08-10', '2025-08-25', N'Vòng 1', 2, NULL, N'Khả thi', '2025-08-21'),
(12, 15, 1, 9, 1, 1, '2025-08-10', '2025-08-25', N'Vòng 1', 2, NULL, N'Cần cải thiện giao diện', '2025-08-23'),
(13, 16, 2, 7, 1, 1, '2025-08-15', '2025-08-30', N'Vòng 1', 2, NULL, N'Sáng tạo', '2025-08-25'),
(14, 16, 2, 10, 1, 1, '2025-08-15', '2025-08-30', N'Vòng 1', 2, NULL, N'OK', '2025-08-26'),
(15, 17, 3, 8, 2, 1, '2025-08-18', '2025-09-02', N'Vòng 1', 2, NULL, N'Tiết kiệm', '2025-08-28'),
(16, 17, 3, 9, 2, 1, '2025-08-18', '2025-09-02', N'Vòng 1', 2, NULL, N'Hiệu quả cao', '2025-08-29'),

-- Approved (completed)
(17, 18, 1, 7, 1, 1, '2024-09-15', '2024-09-30', N'Vòng 1', 2, 1, N'Xuất sắc', '2024-09-28'),
(18, 18, 1, 8, 1, 1, '2024-09-15', '2024-09-30', N'Vòng 1', 2, 1, N'Rất tốt', '2024-09-29'),
(19, 19, 1, 7, 1, 1, '2024-09-25', '2024-10-10', N'Vòng 1', 2, 1, N'Tuyệt vời', '2024-10-05'),
(20, 19, 1, 9, 1, 1, '2024-09-25', '2024-10-10', N'Vòng 1', 2, 1, N'Đạt yêu cầu', '2024-10-06')
SET IDENTITY_INSERT [dbo].[InitiativeAssignments] OFF
GO

-- ========================================
-- 15. EVALUATION DETAILS (40 rows)
-- ========================================
SET IDENTITY_INSERT [dbo].[EvaluationDetails] ON
INSERT INTO [dbo].[EvaluationDetails] ([Id], [AssignmentId], [CriteriaId], [ScoreGiven], [Note]) VALUES
-- Assignment 1 (Initiative 11)
(1, 1, 1, 22, N'Ý tưởng mới'), (2, 1, 2, 18, N'Có thể áp dụng'), (3, 1, 3, 23, N'Tiết kiệm tốt'), (4, 1, 4, 13, N'Nhân rộng được'), (5, 1, 5, 14, N'Hồ sơ rõ ràng'),
-- Assignment 2 (Initiative 11)
(6, 2, 1, 20, N'Khá mới'), (7, 2, 2, 17, N'Khả thi'), (8, 2, 3, 21, N'Hiệu quả'), (9, 2, 4, 12, N'OK'), (10, 2, 5, 13, N'Cần bổ sung'),
-- Assignment 10-12 (Initiative 15)
(11, 10, 1, 24, N'Xuất sắc'), (12, 10, 2, 19, N'Tốt'), (13, 10, 3, 24, N'Rất hiệu quả'), (14, 10, 4, 14, N'Dễ nhân rộng'), (15, 10, 5, 15, N'Hoàn chỉnh'),
(16, 11, 1, 23, N'Hay'), (17, 11, 2, 18, N'Khả thi cao'), (18, 11, 3, 22, N'Tốt'), (19, 11, 4, 13, N'OK'), (20, 11, 5, 14, N'Đầy đủ'),
(21, 12, 1, 21, N'Sáng tạo'), (22, 12, 2, 17, N'Có thể làm'), (23, 12, 3, 20, N'Hiệu quả'), (24, 12, 4, 12, N'Cần xem xét'), (25, 12, 5, 13, N'OK'),
-- Assignment 17-18 (Initiative 18 - Approved)
(26, 17, 1, 25, N'Xuất sắc'), (27, 17, 2, 20, N'Rất khả thi'), (28, 17, 3, 25, N'Tiết kiệm cao'), (29, 17, 4, 15, N'Nhân rộng tốt'), (30, 17, 5, 15, N'Hoàn hảo'),
(31, 18, 1, 24, N'Hay'), (32, 18, 2, 19, N'OK'), (33, 18, 3, 24, N'Tốt'), (34, 18, 4, 14, N'Được'), (35, 18, 5, 14, N'Đầy đủ'),
-- Assignment 19-20 (Initiative 19 - Approved)
(36, 19, 1, 25, N'Rất mới'), (37, 19, 2, 20, N'Khả thi'), (38, 19, 3, 23, N'Hiệu quả'), (39, 19, 4, 14, N'OK'), (40, 19, 5, 15, N'Tốt')
SET IDENTITY_INSERT [dbo].[EvaluationDetails] OFF
GO

-- ========================================
-- 16. FINAL RESULTS (8 rows)
-- ========================================
SET IDENTITY_INSERT [dbo].[FinalResults] ON
INSERT INTO [dbo].[FinalResults] ([Id], [InitiativeId], [AverageScore], [ChairmanDecision], [ChairmanComment], [DecidedAt], [ChairmanId]) VALUES
(1, 18, 97.5, N'Xuất sắc', N'Đề tài rất hay, cần nhân rộng', '2024-10-15', 7),
(2, 19, 95.0, N'Xuất sắc', N'Công nghệ tốt', '2024-10-20', 7),
(3, 20, 88.0, N'Giỏi', N'Sáng tạo', '2024-10-25', 7),
(4, 21, 85.0, N'Giỏi', N'Hiệu quả cao', '2024-10-30', 8),
(5, 22, 82.0, N'Khá', N'Cần cải thiện', '2024-11-01', 8),
(6, 23, 45.0, N'Không đạt', N'Chi phí quá cao, không khả thi', '2024-11-05', 7),
(7, 24, 52.0, N'Không đạt', N'Công nghệ chưa phù hợp', '2024-11-10', 7),
(8, 25, 48.0, N'Không đạt', N'Trùng lặp với hệ thống hiện có', '2024-11-15', 8)
SET IDENTITY_INSERT [dbo].[FinalResults] OFF
GO

-- ========================================
-- 17. REFERENCE FORMS (6 rows)
-- ========================================
SET IDENTITY_INSERT [dbo].[ReferenceForms] ON
INSERT INTO [dbo].[ReferenceForms] ([Id], [PeriodId], [FileName], [Description], [FileUrl], [UploadedAt]) VALUES
(1, 3, N'Mẫu đơn đăng ký sáng kiến.docx', N'Mẫu đơn đăng ký sáng kiến chính thức', 'https://example.com/forms/form1.docx', '2025-08-20'),
(2, 3, N'Hướng dẫn viết thuyết minh.pdf', N'Hướng dẫn chi tiết cách viết thuyết minh sáng kiến', 'https://example.com/forms/guide.pdf', '2025-08-20'),
(3, 3, N'Mẫu báo cáo kết quả.xlsx', N'Mẫu báo cáo kết quả áp dụng sáng kiến', 'https://example.com/forms/report.xlsx', '2025-08-21'),
(4, 3, N'Tiêu chí đánh giá sáng kiến.pdf', N'Tiêu chí và thang điểm đánh giá', 'https://example.com/forms/criteria.pdf', '2025-08-22'),
(5, 1, N'Mẫu đơn HK1 2024.docx', N'Mẫu đơn đợt 1', 'https://example.com/forms/old_form1.docx', '2024-08-15'),
(6, 1, N'Hướng dẫn HK1 2024.pdf', N'Hướng dẫn đợt 1', 'https://example.com/forms/old_guide.pdf', '2024-08-15')
SET IDENTITY_INSERT [dbo].[ReferenceForms] OFF
GO

PRINT N'Sample data inserted successfully!'
GO
