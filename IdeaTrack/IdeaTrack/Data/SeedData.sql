-- ========================================
-- IDEATRACK EXPANDED SAMPLE DATA SEED SCRIPT
-- Run this script after database migration
-- Target: 20+ records per table for realistic testing
-- ========================================

USE [IdeaTrackDB]
GO

-- ========================================
-- 1. DEPARTMENTS (10 rows) ✓
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
-- 3. USERS (30 rows) ✓
-- Password: "123456" - Hash: AQAAAAIAAYagAAAAEBs0rDxMzP0vlZB6v6gNlAzUmSPpN5ot3wl1T1FQJvqwJ9qLQQQQQQQQQQQQQQQQQQ==
-- ========================================
SET IDENTITY_INSERT [dbo].[AspNetUsers] ON
INSERT INTO [dbo].[AspNetUsers] ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [FullName], [DepartmentId], [EmployeeCode]) VALUES
-- Admin & SciTech (3)
(1, 'admin', 'ADMIN', 'admin@uni.edu.vn', 'ADMIN@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000001', 0, 0, NULL, 0, 0, N'Quản trị viên', 7, 'AD001'),
(2, 'scitech1', 'SCITECH1', 'scitech1@uni.edu.vn', 'SCITECH1@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000002', 0, 0, NULL, 0, 0, N'Nguyễn Văn KHCN', 7, 'ST001'),
(3, 'scitech2', 'SCITECH2', 'scitech2@uni.edu.vn', 'SCITECH2@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000003', 0, 0, NULL, 0, 0, N'Trần Thị KHCN', 7, 'ST002'),

-- Faculty Leaders (6)
(4, 'leader_cntt', 'LEADER_CNTT', 'leader.cntt@uni.edu.vn', 'LEADER.CNTT@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000004', 0, 0, NULL, 0, 0, N'PGS.TS Lê Văn An', 1, 'FL001'),
(5, 'leader_dt', 'LEADER_DT', 'leader.dt@uni.edu.vn', 'LEADER.DT@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000005', 0, 0, NULL, 0, 0, N'TS. Phạm Văn Bình', 2, 'FL002'),
(6, 'leader_ck', 'LEADER_CK', 'leader.ck@uni.edu.vn', 'LEADER.CK@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000006', 0, 0, NULL, 0, 0, N'ThS. Hoàng Văn Cường', 3, 'FL003'),
(7, 'leader_kt', 'LEADER_KT', 'leader.kt@uni.edu.vn', 'LEADER.KT@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000007', 0, 0, NULL, 0, 0, N'PGS.TS Ngô Thị Diệu', 4, 'FL004'),
(8, 'leader_nn', 'LEADER_NN', 'leader.nn@uni.edu.vn', 'LEADER.NN@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000008', 0, 0, NULL, 0, 0, N'TS. Vũ Minh Đức', 5, 'FL005'),
(9, 'leader_xd', 'LEADER_XD', 'leader.xd@uni.edu.vn', 'LEADER.XD@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000009', 0, 0, NULL, 0, 0, N'ThS. Lý Văn Giang', 8, 'FL006'),

-- Council Members (10)
(10, 'council1', 'COUNCIL1', 'council1@uni.edu.vn', 'COUNCIL1@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000010', 0, 0, NULL, 0, 0, N'GS.TS Nguyễn Hữu Đức', 10, 'CM001'),
(11, 'council2', 'COUNCIL2', 'council2@uni.edu.vn', 'COUNCIL2@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000011', 0, 0, NULL, 0, 0, N'PGS.TS Trần Mai Hoa', 1, 'CM002'),
(12, 'council3', 'COUNCIL3', 'council3@uni.edu.vn', 'COUNCIL3@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000012', 0, 0, NULL, 0, 0, N'TS. Lê Minh Giang', 2, 'CM003'),
(13, 'council4', 'COUNCIL4', 'council4@uni.edu.vn', 'COUNCIL4@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000013', 0, 0, NULL, 0, 0, N'TS. Phạm Thị Hằng', 4, 'CM004'),
(14, 'council5', 'COUNCIL5', 'council5@uni.edu.vn', 'COUNCIL5@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000014', 0, 0, NULL, 0, 0, N'ThS. Vũ Văn Khánh', 3, 'CM005'),
(15, 'council6', 'COUNCIL6', 'council6@uni.edu.vn', 'COUNCIL6@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000015', 0, 0, NULL, 0, 0, N'PGS.TS Đỗ Văn Long', 1, 'CM006'),
(16, 'council7', 'COUNCIL7', 'council7@uni.edu.vn', 'COUNCIL7@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000016', 0, 0, NULL, 0, 0, N'TS. Lý Thị Mai', 5, 'CM007'),
(17, 'council8', 'COUNCIL8', 'council8@uni.edu.vn', 'COUNCIL8@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000017', 0, 0, NULL, 0, 0, N'ThS. Hoàng Văn Nam', 8, 'CM008'),
(18, 'council9', 'COUNCIL9', 'council9@uni.edu.vn', 'COUNCIL9@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000018', 0, 0, NULL, 0, 0, N'TS. Ngô Thị Oanh', 9, 'CM009'),
(19, 'council10', 'COUNCIL10', 'council10@uni.edu.vn', 'COUNCIL10@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000019', 0, 0, NULL, 0, 0, N'PGS.TS Trần Văn Phúc', 6, 'CM010'),

-- Authors/Lecturers (11)
(20, 'author1', 'AUTHOR1', 'author1@uni.edu.vn', 'AUTHOR1@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000020', 0, 0, NULL, 0, 0, N'ThS. Nguyễn Văn Linh', 1, 'AU001'),
(21, 'author2', 'AUTHOR2', 'author2@uni.edu.vn', 'AUTHOR2@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000021', 0, 0, NULL, 0, 0, N'ThS. Trần Thị Mai', 1, 'AU002'),
(22, 'author3', 'AUTHOR3', 'author3@uni.edu.vn', 'AUTHOR3@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000022', 0, 0, NULL, 0, 0, N'TS. Lê Hoàng Nam', 2, 'AU003'),
(23, 'author4', 'AUTHOR4', 'author4@uni.edu.vn', 'AUTHOR4@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000023', 0, 0, NULL, 0, 0, N'ThS. Phạm Văn Oanh', 2, 'AU004'),
(24, 'author5', 'AUTHOR5', 'author5@uni.edu.vn', 'AUTHOR5@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000024', 0, 0, NULL, 0, 0, N'ThS. Hoàng Thị Phương', 3, 'AU005'),
(25, 'author6', 'AUTHOR6', 'author6@uni.edu.vn', 'AUTHOR6@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000025', 0, 0, NULL, 0, 0, N'TS. Nguyễn Văn Quân', 4, 'AU006'),
(26, 'author7', 'AUTHOR7', 'author7@uni.edu.vn', 'AUTHOR7@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000026', 0, 0, NULL, 0, 0, N'ThS. Trần Văn Sơn', 5, 'AU007'),
(27, 'author8', 'AUTHOR8', 'author8@uni.edu.vn', 'AUTHOR8@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000027', 0, 0, NULL, 0, 0, N'ThS. Lê Thị Trang', 6, 'AU008'),
(28, 'author9', 'AUTHOR9', 'author9@uni.edu.vn', 'AUTHOR9@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000028', 0, 0, NULL, 0, 0, N'TS. Vũ Minh Uyên', 1, 'AU009'),
(29, 'author10', 'AUTHOR10', 'author10@uni.edu.vn', 'AUTHOR10@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000029', 0, 0, NULL, 0, 0, N'ThS. Đỗ Văn Việt', 8, 'AU010'),
(30, 'author11', 'AUTHOR11', 'author11@uni.edu.vn', 'AUTHOR11@UNI.EDU.VN', 1, 'AQAAAAIAAYagAAAAEA...', NEWID(), NEWID(), '0901000030', 0, 0, NULL, 0, 0, N'ThS. Lý Thị Xuân', 9, 'AU011')
SET IDENTITY_INSERT [dbo].[AspNetUsers] OFF
GO

-- ========================================
-- 4. USER ROLES
-- ========================================
INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES
(1, 1), -- admin -> Admin
(2, 2), -- scitech1 -> SciTech
(3, 2), -- scitech2 -> SciTech
(4, 3), (5, 3), (6, 3), (7, 3), (8, 3), (9, 3), -- Faculty Leaders
(10, 4), (11, 4), (12, 4), (13, 4), (14, 4), (15, 4), (16, 4), (17, 4), (18, 4), (19, 4), -- Council Members
(20, 5), (20, 6), (21, 5), (21, 6), (22, 5), (22, 6), (23, 5), (23, 6), (24, 5), (24, 6),
(25, 5), (25, 6), (26, 5), (26, 6), (27, 5), (27, 6), (28, 5), (28, 6), (29, 5), (29, 6), (30, 5), (30, 6)
GO

-- ========================================
-- 5. ACADEMIC YEARS (5 rows) ✓
-- ========================================
SET IDENTITY_INSERT [dbo].[AcademicYears] ON
INSERT INTO [dbo].[AcademicYears] ([Id], [Name], [StartDate], [EndDate], [IsCurrent]) VALUES
(1, N'Năm học 2022-2023', '2022-09-01', '2023-08-31', 0),
(2, N'Năm học 2023-2024', '2023-09-01', '2024-08-31', 0),
(3, N'Năm học 2024-2025', '2024-09-01', '2025-08-31', 0),
(4, N'Năm học 2025-2026', '2025-09-01', '2026-08-31', 1),
(5, N'Năm học 2026-2027', '2026-09-01', '2027-08-31', 0)
SET IDENTITY_INSERT [dbo].[AcademicYears] OFF
GO

-- ========================================
-- 6. INITIATIVE PERIODS (10 rows) ✓
-- Note: IsOpen determined by StartDate <= Today <= EndDate
-- ========================================
SET IDENTITY_INSERT [dbo].[InitiativePeriods] ON
INSERT INTO [dbo].[InitiativePeriods] ([Id], [Name], [Description], [StartDate], [EndDate], [IsActive], [AcademicYearId], [CreatedAt]) VALUES
-- Historical periods (closed)
(1, N'Đợt Sáng kiến HK1 2022-2023', N'Đợt 1 năm 2022-2023', '2022-09-01', '2022-12-31', 0, 1, '2022-08-15'),
(2, N'Đợt Sáng kiến HK2 2022-2023', N'Đợt 2 năm 2022-2023', '2023-01-01', '2023-06-30', 0, 1, '2022-12-15'),
(3, N'Đợt Sáng kiến HK1 2023-2024', N'Đợt 1 năm 2023-2024', '2023-09-01', '2023-12-31', 0, 2, '2023-08-15'),
(4, N'Đợt Sáng kiến HK2 2023-2024', N'Đợt 2 năm 2023-2024', '2024-01-01', '2024-06-30', 0, 2, '2023-12-15'),
(5, N'Đợt Sáng kiến HK1 2024-2025', N'Đợt 1 năm 2024-2025', '2024-09-01', '2024-12-31', 0, 3, '2024-08-15'),
(6, N'Đợt Sáng kiến HK2 2024-2025', N'Đợt 2 năm 2024-2025', '2025-01-01', '2025-06-30', 0, 3, '2024-12-15'),
-- Current periods (OPEN based on date 2026-01-06)
(7, N'Đợt Sáng kiến HK1 2025-2026', N'Đợt 1 năm 2025-2026 - ĐANG MỞ', '2025-09-01', '2026-01-31', 1, 4, '2025-08-15'),
(8, N'Đợt Sáng kiến Đặc biệt 2025-2026', N'Đợt đặc biệt - ĐANG MỞ', '2025-12-01', '2026-02-28', 1, 4, '2025-11-15'),
-- Future periods
(9, N'Đợt Sáng kiến HK2 2025-2026', N'Đợt 2 năm 2025-2026', '2026-02-01', '2026-06-30', 0, 4, '2026-01-01'),
(10, N'Đợt Sáng kiến HK1 2026-2027', N'Đợt 1 năm 2026-2027', '2026-09-01', '2026-12-31', 0, 5, '2026-08-15')
SET IDENTITY_INSERT [dbo].[InitiativePeriods] OFF
GO

-- ========================================
-- 7. EVALUATION TEMPLATES (5 rows) ✓
-- ========================================
SET IDENTITY_INSERT [dbo].[EvaluationTemplates] ON
INSERT INTO [dbo].[EvaluationTemplates] ([Id], [TemplateName], [Description], [Type], [IsActive]) VALUES
(1, N'Mẫu chấm Sáng kiến Kỹ thuật', N'Dùng cho các sáng kiến kỹ thuật, công nghệ - 100 điểm', 1, 1),
(2, N'Mẫu chấm Sáng kiến Quản lý', N'Dùng cho các sáng kiến quản lý, quy trình - 100 điểm', 1, 1),
(3, N'Mẫu sơ duyệt Khoa', N'Mẫu sơ duyệt cấp Khoa - 100 điểm', 0, 1),
(4, N'Mẫu chấm Sáng kiến Phần mềm', N'Dùng cho các sáng kiến phần mềm, ứng dụng - 100 điểm', 1, 1),
(5, N'Mẫu chấm Sáng kiến Giáo dục', N'Dùng cho các sáng kiến phương pháp giảng dạy - 100 điểm', 1, 1)
SET IDENTITY_INSERT [dbo].[EvaluationTemplates] OFF
GO

-- ========================================
-- 8. EVALUATION CRITERIA (25 rows) ✓
-- ========================================
SET IDENTITY_INSERT [dbo].[EvaluationCriteria] ON
INSERT INTO [dbo].[EvaluationCriteria] ([Id], [TemplateId], [CriteriaName], [Description], [MaxScore], [SortOrder]) VALUES
-- Template 1: Kỹ thuật (5 criteria)
(1, 1, N'Tính mới', N'Mức độ mới mẻ, sáng tạo của giải pháp', 25, 1),
(2, 1, N'Tính khả thi', N'Khả năng áp dụng vào thực tế', 20, 2),
(3, 1, N'Hiệu quả kinh tế', N'Lợi ích kinh tế mang lại', 25, 3),
(4, 1, N'Khả năng nhân rộng', N'Có thể áp dụng ở nhiều nơi', 15, 4),
(5, 1, N'Chất lượng hồ sơ', N'Tính đầy đủ, rõ ràng của hồ sơ', 15, 5),
-- Template 2: Quản lý (5 criteria)
(6, 2, N'Tính mới', N'Mức độ đổi mới trong quy trình', 20, 1),
(7, 2, N'Tính hiệu quả', N'Cải thiện hiệu suất công việc', 25, 2),
(8, 2, N'Tiết kiệm chi phí', N'Giảm chi phí hoạt động', 20, 3),
(9, 2, N'Dễ triển khai', N'Dễ dàng áp dụng', 20, 4),
(10, 2, N'Chất lượng hồ sơ', N'Tính đầy đủ, rõ ràng', 15, 5),
-- Template 3: Sơ duyệt (3 criteria)
(11, 3, N'Đủ điều kiện nộp', N'Đáp ứng các yêu cầu cơ bản', 30, 1),
(12, 3, N'Tính khả thi sơ bộ', N'Đánh giá sơ bộ tính khả thi', 30, 2),
(13, 3, N'Hồ sơ đầy đủ', N'Có đầy đủ các biểu mẫu', 40, 3),
-- Template 4: Phần mềm (5 criteria)
(14, 4, N'Tính sáng tạo', N'Mức độ sáng tạo trong giải pháp công nghệ', 25, 1),
(15, 4, N'Khả năng mở rộng', N'Hệ thống có thể mở rộng', 20, 2),
(16, 4, N'Trải nghiệm người dùng', N'Giao diện thân thiện, dễ sử dụng', 20, 3),
(17, 4, N'Bảo mật', N'Độ an toàn của hệ thống', 15, 4),
(18, 4, N'Tài liệu kỹ thuật', N'Hồ sơ kỹ thuật đầy đủ', 20, 5),
-- Template 5: Giáo dục (7 criteria)
(19, 5, N'Tính mới phương pháp', N'Đổi mới phương pháp giảng dạy', 20, 1),
(20, 5, N'Hiệu quả học tập', N'Cải thiện kết quả học tập', 20, 2),
(21, 5, N'Tương tác sinh viên', N'Tăng tương tác với sinh viên', 15, 3),
(22, 5, N'Ứng dụng công nghệ', N'Sử dụng công nghệ hiệu quả', 15, 4),
(23, 5, N'Dễ triển khai', N'Có thể áp dụng rộng rãi', 15, 5),
(24, 5, N'Tài liệu hỗ trợ', N'Giáo trình, bài giảng đầy đủ', 10, 6),
(25, 5, N'Phản hồi tích cực', N'Sinh viên đánh giá tốt', 5, 7)
SET IDENTITY_INSERT [dbo].[EvaluationCriteria] OFF
GO

-- ========================================
-- 9. BOARDS (5 rows) ✓
-- ========================================
SET IDENTITY_INSERT [dbo].[Boards] ON
INSERT INTO [dbo].[Boards] ([Id], [BoardName], [FiscalYear], [Description], [IsActive], [CreatedAt]) VALUES
(1, N'Hội đồng Sáng kiến CNTT', 2026, N'Chấm sáng kiến lĩnh vực CNTT', 1, '2025-08-01'),
(2, N'Hội đồng Sáng kiến Kỹ thuật', 2026, N'Chấm sáng kiến lĩnh vực Kỹ thuật, Cơ khí, Điện', 1, '2025-08-01'),
(3, N'Hội đồng Sáng kiến Quản lý', 2026, N'Chấm sáng kiến lĩnh vực Quản lý, Kinh tế', 1, '2025-08-01'),
(4, N'Hội đồng Sáng kiến Giáo dục', 2026, N'Chấm sáng kiến phương pháp giảng dạy', 1, '2025-08-01'),
(5, N'Hội đồng Sáng kiến Đa ngành', 2026, N'Chấm sáng kiến liên ngành, tổng hợp', 1, '2025-08-01')
SET IDENTITY_INSERT [dbo].[Boards] OFF
GO

-- ========================================
-- 10. BOARD MEMBERS (25 rows) ✓
-- Role: 0=Chairman, 1=Secretary, 2=Member
-- ========================================
SET IDENTITY_INSERT [dbo].[BoardMembers] ON
INSERT INTO [dbo].[BoardMembers] ([Id], [BoardId], [UserId], [Role], [JoinDate]) VALUES
-- Board 1: CNTT (5 members)
(1, 1, 10, 0, '2025-08-01'),  -- Chairman
(2, 1, 11, 1, '2025-08-01'),  -- Secretary
(3, 1, 12, 2, '2025-08-01'),  -- Member
(4, 1, 15, 2, '2025-08-01'),  -- Member
(5, 1, 19, 2, '2025-08-01'),  -- Member
-- Board 2: Kỹ thuật (5 members)
(6, 2, 10, 0, '2025-08-01'),  -- Chairman
(7, 2, 14, 1, '2025-08-01'),  -- Secretary
(8, 2, 12, 2, '2025-08-01'),  -- Member
(9, 2, 17, 2, '2025-08-01'),  -- Member
(10, 2, 18, 2, '2025-08-01'), -- Member
-- Board 3: Quản lý (5 members)
(11, 3, 11, 0, '2025-08-01'), -- Chairman
(12, 3, 13, 1, '2025-08-01'), -- Secretary
(13, 3, 16, 2, '2025-08-01'), -- Member
(14, 3, 18, 2, '2025-08-01'), -- Member
(15, 3, 19, 2, '2025-08-01'), -- Member
-- Board 4: Giáo dục (5 members)
(16, 4, 16, 0, '2025-08-01'), -- Chairman
(17, 4, 11, 1, '2025-08-01'), -- Secretary
(18, 4, 13, 2, '2025-08-01'), -- Member
(19, 4, 15, 2, '2025-08-01'), -- Member
(20, 4, 19, 2, '2025-08-01'), -- Member
-- Board 5: Đa ngành (5 members)
(21, 5, 10, 0, '2025-08-01'), -- Chairman
(22, 5, 15, 1, '2025-08-01'), -- Secretary
(23, 5, 14, 2, '2025-08-01'), -- Member
(24, 5, 16, 2, '2025-08-01'), -- Member
(25, 5, 17, 2, '2025-08-01')  -- Member
SET IDENTITY_INSERT [dbo].[BoardMembers] OFF
GO

-- ========================================
-- 11. INITIATIVE CATEGORIES (30 rows) ✓
-- Each category links to 1 Board + 1 Template (1-1-1 mechanism)
-- ========================================
SET IDENTITY_INSERT [dbo].[InitiativeCategories] ON
INSERT INTO [dbo].[InitiativeCategories] ([Id], [PeriodId], [Name], [Description], [BoardId], [TemplateId]) VALUES
-- Period 7 (Current 2025-2026 HK1) - 6 categories
(1, 7, N'Sáng kiến Phần mềm', N'Các sáng kiến về phần mềm, ứng dụng', 1, 4),
(2, 7, N'Sáng kiến Phần cứng/IoT', N'Các sáng kiến về thiết bị, IoT', 2, 1),
(3, 7, N'Sáng kiến Tự động hóa', N'Các sáng kiến tự động hóa quy trình sản xuất', 2, 1),
(4, 7, N'Sáng kiến Quản lý đào tạo', N'Cải tiến quy trình đào tạo', 3, 2),
(5, 7, N'Sáng kiến Quản lý hành chính', N'Cải tiến quy trình hành chính', 3, 2),
(6, 7, N'Sáng kiến Phương pháp giảng dạy', N'Đổi mới phương pháp giảng dạy', 4, 5),
-- Period 8 (Special period) - 4 categories
(7, 8, N'Sáng kiến AI/ML', N'Sáng kiến ứng dụng Trí tuệ nhân tạo', 1, 4),
(8, 8, N'Sáng kiến Năng lượng xanh', N'Các sáng kiến tiết kiệm năng lượng', 5, 1),
(9, 8, N'Sáng kiến Chuyển đổi số', N'Chuyển đổi số trong quản lý', 3, 2),
(10, 8, N'Sáng kiến Liên ngành', N'Sáng kiến kết hợp nhiều lĩnh vực', 5, 1),
-- Period 5 (2024-2025 HK1) - 5 categories
(11, 5, N'Sáng kiến CNTT', N'Sáng kiến CNTT HK1 2024', 1, 1),
(12, 5, N'Sáng kiến Kỹ thuật', N'Sáng kiến Kỹ thuật HK1 2024', 2, 1),
(13, 5, N'Sáng kiến Quản lý', N'Sáng kiến Quản lý HK1 2024', 3, 2),
(14, 5, N'Sáng kiến Giáo dục', N'Sáng kiến Giáo dục HK1 2024', 4, 5),
(15, 5, N'Sáng kiến Đa ngành', N'Sáng kiến Đa ngành HK1 2024', 5, 1),
-- Period 6 (2024-2025 HK2) - 5 categories
(16, 6, N'Sáng kiến CNTT HK2', N'Sáng kiến CNTT HK2 2024', 1, 4),
(17, 6, N'Sáng kiến Kỹ thuật HK2', N'Sáng kiến Kỹ thuật HK2 2024', 2, 1),
(18, 6, N'Sáng kiến Quản lý HK2', N'Sáng kiến Quản lý HK2 2024', 3, 2),
(19, 6, N'Sáng kiến Giáo dục HK2', N'Sáng kiến Giáo dục HK2 2024', 4, 5),
(20, 6, N'Sáng kiến Startup', N'Ý tưởng khởi nghiệp', 5, 1),
-- Period 3 (2023-2024 HK1) - 5 categories
(21, 3, N'Sáng kiến CNTT 2023', N'Sáng kiến CNTT 2023', 1, 1),
(22, 3, N'Sáng kiến Kỹ thuật 2023', N'Sáng kiến Kỹ thuật 2023', 2, 1),
(23, 3, N'Sáng kiến Quản lý 2023', N'Sáng kiến Quản lý 2023', 3, 2),
(24, 3, N'Sáng kiến Giáo dục 2023', N'Sáng kiến Giáo dục 2023', 4, 5),
(25, 3, N'Sáng kiến Đa ngành 2023', N'Sáng kiến Đa ngành 2023', 5, 1),
-- Period 4 (2023-2024 HK2) - 5 categories
(26, 4, N'Sáng kiến CNTT HK2 2023', N'Sáng kiến CNTT HK2 2023', 1, 4),
(27, 4, N'Sáng kiến Kỹ thuật HK2 2023', N'Sáng kiến Kỹ thuật HK2 2023', 2, 1),
(28, 4, N'Sáng kiến Quản lý HK2 2023', N'Sáng kiến Quản lý HK2 2023', 3, 2),
(29, 4, N'Sáng kiến Môi trường', N'Sáng kiến bảo vệ môi trường', 5, 1),
(30, 4, N'Sáng kiến An toàn lao động', N'Cải thiện an toàn lao động', 2, 1)
SET IDENTITY_INSERT [dbo].[InitiativeCategories] OFF
GO

-- ========================================
-- 12. REFERENCE FORMS (10 rows) ✓
-- ========================================
SET IDENTITY_INSERT [dbo].[ReferenceForms] ON
INSERT INTO [dbo].[ReferenceForms] ([Id], [PeriodId], [FileName], [Description], [FileUrl], [UploadedAt]) VALUES
-- Current period 7
(1, 7, N'Mẫu đơn đăng ký sáng kiến.docx', N'Mẫu đơn đăng ký sáng kiến chính thức', 'https://supabase.io/storage/forms/form1.docx', '2025-08-20'),
(2, 7, N'Hướng dẫn viết thuyết minh.pdf', N'Hướng dẫn chi tiết cách viết thuyết minh sáng kiến', 'https://supabase.io/storage/forms/guide.pdf', '2025-08-20'),
(3, 7, N'Mẫu báo cáo kết quả.xlsx', N'Mẫu báo cáo kết quả áp dụng sáng kiến', 'https://supabase.io/storage/forms/report.xlsx', '2025-08-21'),
(4, 7, N'Tiêu chí đánh giá sáng kiến.pdf', N'Tiêu chí và thang điểm đánh giá', 'https://supabase.io/storage/forms/criteria.pdf', '2025-08-22'),
(5, 7, N'Quy chế sáng kiến 2025.pdf', N'Quy chế công nhận sáng kiến năm 2025', 'https://supabase.io/storage/forms/policy.pdf', '2025-08-22'),
-- Special period 8
(6, 8, N'Mẫu đăng ký AI-ML.docx', N'Mẫu đăng ký sáng kiến AI/ML', 'https://supabase.io/storage/forms/ai_form.docx', '2025-11-20'),
(7, 8, N'Hướng dẫn sáng kiến đặc biệt.pdf', N'Hướng dẫn đợt đặc biệt', 'https://supabase.io/storage/forms/special_guide.pdf', '2025-11-20'),
-- Historical period 5
(8, 5, N'Mẫu đơn HK1 2024.docx', N'Mẫu đơn đợt 1 năm 2024', 'https://supabase.io/storage/forms/old_form1.docx', '2024-08-15'),
(9, 5, N'Hướng dẫn HK1 2024.pdf', N'Hướng dẫn đợt 1 năm 2024', 'https://supabase.io/storage/forms/old_guide.pdf', '2024-08-15'),
(10, 5, N'Tiêu chí HK1 2024.pdf', N'Tiêu chí đợt 1 năm 2024', 'https://supabase.io/storage/forms/old_criteria.pdf', '2024-08-16')
SET IDENTITY_INSERT [dbo].[ReferenceForms] OFF
GO

-- ========================================
-- 13. INITIATIVES (25 rows) ✓
-- Status: 0=Draft, 1=Pending, 2=Faculty_Approved, 3=OST_Approved, 4=Evaluating, 5=Revision_Required, 6=Pending_Final, 7=Approved, 8=Rejected
-- ========================================
SET IDENTITY_INSERT [dbo].[Initiatives] ON
INSERT INTO [dbo].[Initiatives] ([Id], [InitiativeCode], [Title], [Description], [Budget], [Status], [CreatedAt], [SubmittedDate], [CurrentRound], [CreatorId], [DepartmentId], [CategoryId], [PeriodId]) VALUES
-- Current period 7: Active initiatives (mixed statuses)
(1, 'SK-2026-0001', N'Hệ thống quản lý điểm danh bằng AI', N'Sử dụng nhận diện khuôn mặt để tự động điểm danh sinh viên', 50000000, 4, '2025-10-01', '2025-10-05', 1, 20, 1, 1, 7),
(2, 'SK-2026-0002', N'Robot dẫn đường tự động trong khuôn viên', N'Robot tự hành giúp hướng dẫn khách tham quan', 150000000, 4, '2025-10-02', '2025-10-10', 1, 22, 2, 2, 7),
(3, 'SK-2026-0003', N'Hệ thống tưới cây thông minh', N'IoT tự động tưới cây theo độ ẩm đất', 30000000, 2, '2025-10-15', '2025-10-20', 1, 24, 3, 2, 7),
(4, 'SK-2026-0004', N'Ứng dụng đăng ký học phần online', N'Cải tiến quy trình đăng ký môn học', 20000000, 1, '2025-11-01', '2025-11-05', 1, 25, 4, 4, 7),
(5, 'SK-2026-0005', N'Phương pháp học từ vựng theo chủ đề', N'Đổi mới cách dạy từ vựng tiếng Anh', 5000000, 6, '2025-09-20', '2025-09-25', 1, 26, 5, 6, 7),
(6, 'SK-2026-0006', N'Hệ thống quản lý tài liệu điện tử', N'Số hóa văn bản hành chính', 80000000, 7, '2025-09-15', '2025-09-18', 1, 21, 1, 1, 7),
(7, 'SK-2026-0007', N'Thiết bị đo chất lượng không khí', N'Cảm biến IoT giám sát môi trường', 45000000, 8, '2025-09-10', '2025-09-12', 1, 23, 2, 2, 7),
(8, 'SK-2026-0008', N'Chatbot hỗ trợ sinh viên 24/7', N'Trợ lý ảo AI trả lời thắc mắc sinh viên', 35000000, 5, '2025-10-20', '2025-10-25', 1, 20, 1, 1, 7),

-- Current period 8: Special period initiatives
(9, 'SK-2026-S001', N'Ứng dụng Machine Learning dự đoán điểm thi', N'AI phân tích dữ liệu học tập dự đoán kết quả', 100000000, 4, '2025-12-10', '2025-12-15', 1, 28, 1, 7, 8),
(10, 'SK-2026-S002', N'Hệ thống năng lượng mặt trời cho giảng đường', N'Pin mặt trời cấp điện cho phòng học', 200000000, 1, '2025-12-20', '2025-12-22', 1, 29, 9, 8, 8),
(11, 'SK-2026-S003', N'Platform chuyển đổi số quản lý đào tạo', N'Hệ thống tích hợp quản lý đào tạo toàn diện', 500000000, 2, '2025-12-05', '2025-12-08', 1, 21, 1, 9, 8),

-- Historical period 5: Completed initiatives
(12, 'SK-2024-0001', N'Website Portal sinh viên', N'Cổng thông tin điện tử cho sinh viên', 40000000, 7, '2024-09-10', '2024-09-15', 1, 20, 1, 11, 5),
(13, 'SK-2024-0002', N'Máy in 3D chi phí thấp', N'Chế tạo máy in 3D từ linh kiện tái chế', 25000000, 7, '2024-09-12', '2024-09-18', 1, 22, 2, 12, 5),
(14, 'SK-2024-0003', N'Quy trình đánh giá năng lực nhân sự', N'Bộ công cụ đánh giá hiệu quả nhân viên', 10000000, 7, '2024-10-01', '2024-10-05', 1, 25, 4, 13, 5),
(15, 'SK-2024-0004', N'Bộ slide bài giảng tương tác', N'Slide trình chiếu kết hợp game hóa', 8000000, 8, '2024-10-10', '2024-10-12', 1, 26, 5, 14, 5),

-- Historical period 6: Mixed statuses
(16, 'SK-2025-0001', N'App quản lý thời gian biểu', N'Ứng dụng mobile quản lý lịch học', 30000000, 7, '2024-12-20', '2024-12-25', 1, 21, 1, 16, 6),
(17, 'SK-2025-0002', N'Robot vệ sinh tự động', N'Robot làm sạch hành lang tự động', 120000000, 7, '2025-01-05', '2025-01-10', 1, 23, 2, 17, 6),
(18, 'SK-2025-0003', N'Hệ thống quản lý kho thông minh', N'Phần mềm quản lý tài sản thiết bị', 60000000, 7, '2025-02-01', '2025-02-05', 1, 25, 4, 18, 6),
(19, 'SK-2025-0004', N'Phương pháp dạy học kết hợp', N'Blended learning cho các môn đại cương', 15000000, 8, '2025-02-15', '2025-02-18', 1, 27, 6, 19, 6),
(20, 'SK-2025-0005', N'Ý tưởng khởi nghiệp EdTech', N'Platform học trực tuyến cho THPT', 250000000, 7, '2025-03-01', '2025-03-05', 1, 28, 1, 20, 6),

-- Draft initiatives (not submitted yet)
(21, NULL, N'Hệ thống nhà xe thông minh', N'Bãi đậu xe tự động tính phí', 80000000, 0, '2026-01-02', NULL, 1, 20, 1, 1, NULL),
(22, NULL, N'VR Lab thực hành Hóa học', N'Phòng thí nghiệm thực tế ảo', 300000000, 0, '2026-01-03', NULL, 1, 27, 6, 6, NULL),
(23, NULL, N'App kiểm tra sức khỏe sinh viên', N'Ứng dụng theo dõi sức khỏe cộng đồng', 50000000, 0, '2026-01-04', NULL, 1, 30, 9, 8, NULL),
(24, NULL, N'Hệ thống đèn LED thông minh', N'Đèn tự động điều chỉnh theo ánh sáng', 70000000, 0, '2026-01-05', NULL, 1, 29, 8, 8, NULL),
(25, NULL, N'Phần mềm chấm bài tự động', N'AI chấm bài tự luận', 90000000, 0, '2026-01-06', NULL, 1, 21, 1, 7, NULL)
SET IDENTITY_INSERT [dbo].[Initiatives] OFF
GO

-- ========================================
-- 14. INITIATIVE AUTHORSHIPS (For multi-author initiatives)
-- Some initiatives have co-authors
-- ========================================
SET IDENTITY_INSERT [dbo].[InitiativeAuthorships] ON
INSERT INTO [dbo].[InitiativeAuthorships] ([Id], [InitiativeId], [UserId], [IsCreator], [ContributionPercentage], [JoinedAt]) VALUES
-- Initiative 1: 2 authors
(1, 1, 20, 1, 60, '2025-10-01'),
(2, 1, 21, 0, 40, '2025-10-02'),
-- Initiative 2: 3 authors
(3, 2, 22, 1, 50, '2025-10-02'),
(4, 2, 23, 0, 30, '2025-10-03'),
(5, 2, 24, 0, 20, '2025-10-04'),
-- Initiative 6: 2 authors
(6, 6, 21, 1, 70, '2025-09-15'),
(7, 6, 20, 0, 30, '2025-09-16'),
-- Initiative 9: 3 authors (AI project)
(8, 9, 28, 1, 40, '2025-12-10'),
(9, 9, 29, 0, 30, '2025-12-11'),
(10, 9, 20, 0, 30, '2025-12-12'),
-- Initiative 11: 2 authors
(11, 11, 21, 1, 60, '2025-12-05'),
(12, 11, 28, 0, 40, '2025-12-06'),
-- Single authors for remaining initiatives
(13, 3, 24, 1, 100, '2025-10-15'),
(14, 4, 25, 1, 100, '2025-11-01'),
(15, 5, 26, 1, 100, '2025-09-20'),
(16, 7, 23, 1, 100, '2025-09-10'),
(17, 8, 20, 1, 100, '2025-10-20'),
(18, 10, 29, 1, 100, '2025-12-20'),
(19, 12, 20, 1, 100, '2024-09-10'),
(20, 13, 22, 1, 100, '2024-09-12')
SET IDENTITY_INSERT [dbo].[InitiativeAuthorships] OFF
GO

PRINT N'✓ Expanded sample data inserted successfully!'
PRINT N'- Departments: 10 rows'
PRINT N'- Users: 30 rows'
PRINT N'- AcademicYears: 5 rows'
PRINT N'- InitiativePeriods: 10 rows'
PRINT N'- EvaluationTemplates: 5 rows'
PRINT N'- EvaluationCriteria: 25 rows'
PRINT N'- Boards: 5 rows'
PRINT N'- BoardMembers: 25 rows'
PRINT N'- InitiativeCategories: 30 rows'
PRINT N'- ReferenceForms: 10 rows'
PRINT N'- Initiatives: 25 rows'
PRINT N'- InitiativeAuthorships: 20 rows'
GO
