/**
 * Comprehensive translation keys for the XenonClinic application.
 * These are the default translations that can be overridden per-tenant.
 */

export interface TranslationDictionary {
  [key: string]: string;
}

export const englishTranslations: TranslationDictionary = {
  // ============================================
  // Common Actions
  // ============================================
  'action.save': 'Save',
  'action.cancel': 'Cancel',
  'action.delete': 'Delete',
  'action.edit': 'Edit',
  'action.create': 'Create',
  'action.add': 'Add',
  'action.remove': 'Remove',
  'action.search': 'Search',
  'action.filter': 'Filter',
  'action.export': 'Export',
  'action.import': 'Import',
  'action.refresh': 'Refresh',
  'action.print': 'Print',
  'action.download': 'Download',
  'action.upload': 'Upload',
  'action.submit': 'Submit',
  'action.confirm': 'Confirm',
  'action.back': 'Back',
  'action.next': 'Next',
  'action.previous': 'Previous',
  'action.close': 'Close',
  'action.view': 'View',
  'action.viewDetails': 'View Details',
  'action.selectAll': 'Select All',
  'action.deselectAll': 'Deselect All',
  'action.reset': 'Reset',
  'action.clear': 'Clear',
  'action.apply': 'Apply',
  'action.login': 'Sign In',
  'action.logout': 'Logout',
  'action.register': 'Register',

  // ============================================
  // Navigation
  // ============================================
  'nav.dashboard': 'Dashboard',
  'nav.appointments': 'Appointments',
  'nav.patients': 'Patients',
  'nav.laboratory': 'Laboratory',
  'nav.pharmacy': 'Pharmacy',
  'nav.radiology': 'Radiology',
  'nav.audiology': 'Audiology',
  'nav.hr': 'HR',
  'nav.financial': 'Financial',
  'nav.inventory': 'Inventory',
  'nav.marketing': 'Marketing',
  'nav.admin': 'Admin',
  'nav.settings': 'Settings',
  'nav.reports': 'Reports',

  // ============================================
  // Entities
  // ============================================
  'entity.patient.singular': 'Patient',
  'entity.patient.plural': 'Patients',
  'entity.appointment.singular': 'Appointment',
  'entity.appointment.plural': 'Appointments',
  'entity.encounter.singular': 'Encounter',
  'entity.encounter.plural': 'Encounters',
  'entity.invoice.singular': 'Invoice',
  'entity.invoice.plural': 'Invoices',
  'entity.prescription.singular': 'Prescription',
  'entity.prescription.plural': 'Prescriptions',
  'entity.labTest.singular': 'Lab Test',
  'entity.labTest.plural': 'Lab Tests',
  'entity.employee.singular': 'Employee',
  'entity.employee.plural': 'Employees',
  'entity.product.singular': 'Product',
  'entity.product.plural': 'Products',
  'entity.order.singular': 'Order',
  'entity.order.plural': 'Orders',

  // ============================================
  // Page Titles & Descriptions
  // ============================================
  'page.dashboard.title': 'Dashboard',
  'page.dashboard.welcome': 'Welcome back',
  'page.dashboard.overview': 'Here\'s your clinic overview across all modules',

  'page.patients.title': 'Patients',
  'page.patients.description': 'Manage patient records and information',
  'page.patients.add': 'New Patient',
  'page.patients.edit': 'Edit Patient',
  'page.patients.recentlyViewed': 'Recently Viewed',
  'page.patients.searchPlaceholder': 'Search by name, Emirates ID, phone, or email...',

  'page.appointments.title': 'Appointments',
  'page.appointments.description': 'Schedule and manage appointments',
  'page.appointments.add': 'New Appointment',
  'page.appointments.schedule': 'Schedule Appointment',

  'page.laboratory.title': 'Laboratory',
  'page.laboratory.description': 'Manage lab tests and results',
  'page.laboratory.pendingOrders': 'Pending Orders',
  'page.laboratory.urgentOrders': 'Urgent Orders',

  'page.pharmacy.title': 'Pharmacy',
  'page.pharmacy.description': 'Manage prescriptions and medications',
  'page.pharmacy.pending': 'Pending Prescriptions',
  'page.pharmacy.dispensedToday': 'Dispensed Today',

  'page.radiology.title': 'Radiology',
  'page.radiology.description': 'Manage imaging orders and reports',

  'page.audiology.title': 'Audiology',
  'page.audiology.description': 'Manage hearing tests and devices',

  'page.hr.title': 'Human Resources',
  'page.hr.description': 'Manage employees and payroll',
  'page.hr.totalEmployees': 'Total Employees',
  'page.hr.activeEmployees': 'Active Employees',

  'page.financial.title': 'Financial',
  'page.financial.description': 'Manage invoices and transactions',
  'page.financial.revenue': 'Revenue',
  'page.financial.unpaidInvoices': 'Unpaid Invoices',

  'page.inventory.title': 'Inventory',
  'page.inventory.description': 'Manage stock and supplies',
  'page.inventory.totalItems': 'Total Items',
  'page.inventory.lowStock': 'Low Stock',

  'page.marketing.title': 'Marketing',
  'page.marketing.description': 'Manage campaigns and leads',

  'page.admin.title': 'Admin',
  'page.admin.description': 'System administration',

  'page.login.title': 'Sign In',
  'page.login.subtitle': 'Healthcare Management System',
  'page.login.welcome': 'Welcome to XenonClinic',
  'page.login.noAccount': 'Don\'t have an account?',
  'page.login.registerHere': 'Register here',
  'page.login.forgotPassword': 'Forgot password?',
  'page.login.rememberMe': 'Remember me',
  'page.login.signingIn': 'Signing in...',

  // ============================================
  // Form Fields
  // ============================================
  'field.firstName': 'First Name',
  'field.lastName': 'Last Name',
  'field.fullName': 'Full Name',
  'field.fullNameEn': 'Full Name (English)',
  'field.fullNameAr': 'Full Name (Arabic)',
  'field.email': 'Email',
  'field.phone': 'Phone',
  'field.phoneNumber': 'Phone Number',
  'field.address': 'Address',
  'field.dateOfBirth': 'Date of Birth',
  'field.gender': 'Gender',
  'field.male': 'Male',
  'field.female': 'Female',
  'field.nationalId': 'National ID',
  'field.emiratesId': 'Emirates ID',
  'field.passportNumber': 'Passport Number',
  'field.nationality': 'Nationality',
  'field.date': 'Date',
  'field.time': 'Time',
  'field.startDate': 'Start Date',
  'field.endDate': 'End Date',
  'field.status': 'Status',
  'field.notes': 'Notes',
  'field.description': 'Description',
  'field.amount': 'Amount',
  'field.quantity': 'Quantity',
  'field.price': 'Price',
  'field.total': 'Total',
  'field.discount': 'Discount',
  'field.tax': 'Tax',
  'field.username': 'Username',
  'field.password': 'Password',
  'field.age': 'Age',
  'field.contact': 'Contact',
  'field.actions': 'Actions',

  // ============================================
  // Status Values
  // ============================================
  'status.active': 'Active',
  'status.inactive': 'Inactive',
  'status.pending': 'Pending',
  'status.completed': 'Completed',
  'status.cancelled': 'Cancelled',
  'status.scheduled': 'Scheduled',
  'status.inProgress': 'In Progress',
  'status.draft': 'Draft',
  'status.approved': 'Approved',
  'status.rejected': 'Rejected',
  'status.paid': 'Paid',
  'status.unpaid': 'Unpaid',
  'status.overdue': 'Overdue',
  'status.booked': 'Booked',
  'status.confirmed': 'Confirmed',
  'status.checkedIn': 'Checked In',
  'status.noShow': 'No Show',

  // ============================================
  // Messages
  // ============================================
  'message.success.saved': 'Saved successfully',
  'message.success.created': 'Created successfully',
  'message.success.updated': 'Updated successfully',
  'message.success.deleted': 'Deleted successfully',
  'message.success.exported': 'Exported successfully',

  'message.error.generic': 'An error occurred',
  'message.error.notFound': 'Item not found',
  'message.error.unauthorized': 'Unauthorized access',
  'message.error.validation': 'Please check the form for errors',
  'message.error.network': 'Network error. Please try again.',
  'message.error.loading': 'Error loading data',

  'message.confirm.delete': 'Are you sure you want to delete this item?',
  'message.confirm.deleteMultiple': 'Are you sure you want to delete {count} item(s)?',
  'message.confirm.unsavedChanges': 'You have unsaved changes. Are you sure you want to leave?',

  'message.loading': 'Loading...',
  'message.noData': 'No data available',
  'message.noResults': 'No results found',
  'message.searchTip': 'Try adjusting your search criteria',
  'message.emptyList': 'No items yet',
  'message.getStarted': 'Get started by creating your first item',

  // ============================================
  // Table & List
  // ============================================
  'table.showing': 'Showing',
  'table.of': 'of',
  'table.results': 'results',
  'table.page': 'Page',
  'table.perPage': 'per page',
  'table.selected': 'selected',
  'table.noData': 'No data to display',
  'table.searchResults': 'Search Results',

  // ============================================
  // Dashboard Stats
  // ============================================
  'stats.today': 'Today',
  'stats.upcoming': 'Upcoming',
  'stats.total': 'Total',
  'stats.new': 'New',
  'stats.thisMonth': 'This Month',
  'stats.monthlyRevenue': 'Monthly Revenue',
  'stats.totalPatients': 'Total Patients',
  'stats.activeStaff': 'Active Staff',
  'stats.systemHealth': 'System Health Status',
  'stats.apiStatus': 'API Status',
  'stats.database': 'Database',
  'stats.modules': 'Modules',
  'stats.alerts': 'Alerts',
  'stats.allOperational': 'All systems operational',
  'stats.connected': 'Connected',
  'stats.needsAttention': 'items need attention',

  // ============================================
  // Clinic Specific
  // ============================================
  'clinic.doctor': 'Doctor',
  'clinic.nurse': 'Nurse',
  'clinic.receptionist': 'Receptionist',
  'clinic.diagnosis': 'Diagnosis',
  'clinic.treatment': 'Treatment',
  'clinic.prescription': 'Prescription',
  'clinic.vitals': 'Vitals',
  'clinic.bloodPressure': 'Blood Pressure',
  'clinic.temperature': 'Temperature',
  'clinic.weight': 'Weight',
  'clinic.height': 'Height',
  'clinic.allergies': 'Allergies',
  'clinic.medicalHistory': 'Medical History',
  'clinic.chiefComplaint': 'Chief Complaint',
  'clinic.followUp': 'Follow Up',

  // ============================================
  // Keyboard Shortcuts
  // ============================================
  'shortcuts.title': 'Keyboard shortcuts',
  'shortcuts.new': 'New',
  'shortcuts.search': 'Search',
  'shortcuts.refresh': 'Refresh',
  'shortcuts.delete': 'Delete selected',
  'shortcuts.tip': 'Tip: Press',
  'shortcuts.toFocusSearch': 'to focus search',

  // ============================================
  // Export Options
  // ============================================
  'export.asCSV': 'Export as CSV',
  'export.asExcel': 'Export as Excel',
  'export.print': 'Print',

  // ============================================
  // Footer
  // ============================================
  'footer.copyright': '© {year} XenonClinic. All rights reserved.',
};

export const arabicTranslations: TranslationDictionary = {
  // ============================================
  // Common Actions
  // ============================================
  'action.save': 'حفظ',
  'action.cancel': 'إلغاء',
  'action.delete': 'حذف',
  'action.edit': 'تعديل',
  'action.create': 'إنشاء',
  'action.add': 'إضافة',
  'action.remove': 'إزالة',
  'action.search': 'بحث',
  'action.filter': 'تصفية',
  'action.export': 'تصدير',
  'action.import': 'استيراد',
  'action.refresh': 'تحديث',
  'action.print': 'طباعة',
  'action.download': 'تحميل',
  'action.upload': 'رفع',
  'action.submit': 'إرسال',
  'action.confirm': 'تأكيد',
  'action.back': 'رجوع',
  'action.next': 'التالي',
  'action.previous': 'السابق',
  'action.close': 'إغلاق',
  'action.view': 'عرض',
  'action.viewDetails': 'عرض التفاصيل',
  'action.selectAll': 'تحديد الكل',
  'action.deselectAll': 'إلغاء تحديد الكل',
  'action.reset': 'إعادة تعيين',
  'action.clear': 'مسح',
  'action.apply': 'تطبيق',
  'action.login': 'تسجيل الدخول',
  'action.logout': 'تسجيل الخروج',
  'action.register': 'التسجيل',

  // ============================================
  // Navigation
  // ============================================
  'nav.dashboard': 'لوحة التحكم',
  'nav.appointments': 'المواعيد',
  'nav.patients': 'المرضى',
  'nav.laboratory': 'المختبر',
  'nav.pharmacy': 'الصيدلية',
  'nav.radiology': 'الأشعة',
  'nav.audiology': 'السمعيات',
  'nav.hr': 'الموارد البشرية',
  'nav.financial': 'المالية',
  'nav.inventory': 'المخزون',
  'nav.marketing': 'التسويق',
  'nav.admin': 'الإدارة',
  'nav.settings': 'الإعدادات',
  'nav.reports': 'التقارير',

  // ============================================
  // Entities
  // ============================================
  'entity.patient.singular': 'مريض',
  'entity.patient.plural': 'المرضى',
  'entity.appointment.singular': 'موعد',
  'entity.appointment.plural': 'المواعيد',
  'entity.encounter.singular': 'زيارة',
  'entity.encounter.plural': 'الزيارات',
  'entity.invoice.singular': 'فاتورة',
  'entity.invoice.plural': 'الفواتير',
  'entity.prescription.singular': 'وصفة طبية',
  'entity.prescription.plural': 'الوصفات الطبية',
  'entity.labTest.singular': 'فحص مخبري',
  'entity.labTest.plural': 'الفحوصات المخبرية',
  'entity.employee.singular': 'موظف',
  'entity.employee.plural': 'الموظفون',
  'entity.product.singular': 'منتج',
  'entity.product.plural': 'المنتجات',
  'entity.order.singular': 'طلب',
  'entity.order.plural': 'الطلبات',

  // ============================================
  // Page Titles & Descriptions
  // ============================================
  'page.dashboard.title': 'لوحة التحكم',
  'page.dashboard.welcome': 'مرحباً بعودتك',
  'page.dashboard.overview': 'نظرة عامة على عيادتك عبر جميع الوحدات',

  'page.patients.title': 'المرضى',
  'page.patients.description': 'إدارة سجلات المرضى والمعلومات',
  'page.patients.add': 'مريض جديد',
  'page.patients.edit': 'تعديل المريض',
  'page.patients.recentlyViewed': 'المشاهدة مؤخراً',
  'page.patients.searchPlaceholder': 'البحث بالاسم أو الهوية الإماراتية أو الهاتف أو البريد الإلكتروني...',

  'page.appointments.title': 'المواعيد',
  'page.appointments.description': 'جدولة وإدارة المواعيد',
  'page.appointments.add': 'موعد جديد',
  'page.appointments.schedule': 'حجز موعد',

  'page.laboratory.title': 'المختبر',
  'page.laboratory.description': 'إدارة الفحوصات والنتائج المخبرية',
  'page.laboratory.pendingOrders': 'الطلبات المعلقة',
  'page.laboratory.urgentOrders': 'الطلبات العاجلة',

  'page.pharmacy.title': 'الصيدلية',
  'page.pharmacy.description': 'إدارة الوصفات والأدوية',
  'page.pharmacy.pending': 'الوصفات المعلقة',
  'page.pharmacy.dispensedToday': 'صُرفت اليوم',

  'page.radiology.title': 'الأشعة',
  'page.radiology.description': 'إدارة طلبات التصوير والتقارير',

  'page.audiology.title': 'السمعيات',
  'page.audiology.description': 'إدارة فحوصات السمع والأجهزة',

  'page.hr.title': 'الموارد البشرية',
  'page.hr.description': 'إدارة الموظفين والرواتب',
  'page.hr.totalEmployees': 'إجمالي الموظفين',
  'page.hr.activeEmployees': 'الموظفون النشطون',

  'page.financial.title': 'المالية',
  'page.financial.description': 'إدارة الفواتير والمعاملات',
  'page.financial.revenue': 'الإيرادات',
  'page.financial.unpaidInvoices': 'فواتير غير مدفوعة',

  'page.inventory.title': 'المخزون',
  'page.inventory.description': 'إدارة المخزون والمستلزمات',
  'page.inventory.totalItems': 'إجمالي العناصر',
  'page.inventory.lowStock': 'مخزون منخفض',

  'page.marketing.title': 'التسويق',
  'page.marketing.description': 'إدارة الحملات والعملاء المحتملين',

  'page.admin.title': 'الإدارة',
  'page.admin.description': 'إدارة النظام',

  'page.login.title': 'تسجيل الدخول',
  'page.login.subtitle': 'نظام إدارة الرعاية الصحية',
  'page.login.welcome': 'مرحباً بك في زينون كلينك',
  'page.login.noAccount': 'ليس لديك حساب؟',
  'page.login.registerHere': 'سجل هنا',
  'page.login.forgotPassword': 'نسيت كلمة المرور؟',
  'page.login.rememberMe': 'تذكرني',
  'page.login.signingIn': 'جاري تسجيل الدخول...',

  // ============================================
  // Form Fields
  // ============================================
  'field.firstName': 'الاسم الأول',
  'field.lastName': 'اسم العائلة',
  'field.fullName': 'الاسم الكامل',
  'field.fullNameEn': 'الاسم الكامل (بالإنجليزية)',
  'field.fullNameAr': 'الاسم الكامل (بالعربية)',
  'field.email': 'البريد الإلكتروني',
  'field.phone': 'الهاتف',
  'field.phoneNumber': 'رقم الهاتف',
  'field.address': 'العنوان',
  'field.dateOfBirth': 'تاريخ الميلاد',
  'field.gender': 'الجنس',
  'field.male': 'ذكر',
  'field.female': 'أنثى',
  'field.nationalId': 'الهوية الوطنية',
  'field.emiratesId': 'الهوية الإماراتية',
  'field.passportNumber': 'رقم جواز السفر',
  'field.nationality': 'الجنسية',
  'field.date': 'التاريخ',
  'field.time': 'الوقت',
  'field.startDate': 'تاريخ البدء',
  'field.endDate': 'تاريخ الانتهاء',
  'field.status': 'الحالة',
  'field.notes': 'ملاحظات',
  'field.description': 'الوصف',
  'field.amount': 'المبلغ',
  'field.quantity': 'الكمية',
  'field.price': 'السعر',
  'field.total': 'الإجمالي',
  'field.discount': 'الخصم',
  'field.tax': 'الضريبة',
  'field.username': 'اسم المستخدم',
  'field.password': 'كلمة المرور',
  'field.age': 'العمر',
  'field.contact': 'الاتصال',
  'field.actions': 'الإجراءات',

  // ============================================
  // Status Values
  // ============================================
  'status.active': 'نشط',
  'status.inactive': 'غير نشط',
  'status.pending': 'معلق',
  'status.completed': 'مكتمل',
  'status.cancelled': 'ملغى',
  'status.scheduled': 'مجدول',
  'status.inProgress': 'قيد التنفيذ',
  'status.draft': 'مسودة',
  'status.approved': 'موافق عليه',
  'status.rejected': 'مرفوض',
  'status.paid': 'مدفوع',
  'status.unpaid': 'غير مدفوع',
  'status.overdue': 'متأخر',
  'status.booked': 'محجوز',
  'status.confirmed': 'مؤكد',
  'status.checkedIn': 'وصل',
  'status.noShow': 'لم يحضر',

  // ============================================
  // Messages
  // ============================================
  'message.success.saved': 'تم الحفظ بنجاح',
  'message.success.created': 'تم الإنشاء بنجاح',
  'message.success.updated': 'تم التحديث بنجاح',
  'message.success.deleted': 'تم الحذف بنجاح',
  'message.success.exported': 'تم التصدير بنجاح',

  'message.error.generic': 'حدث خطأ',
  'message.error.notFound': 'العنصر غير موجود',
  'message.error.unauthorized': 'غير مصرح بالدخول',
  'message.error.validation': 'يرجى التحقق من الأخطاء في النموذج',
  'message.error.network': 'خطأ في الشبكة. يرجى المحاولة مرة أخرى.',
  'message.error.loading': 'خطأ في تحميل البيانات',

  'message.confirm.delete': 'هل أنت متأكد من حذف هذا العنصر؟',
  'message.confirm.deleteMultiple': 'هل أنت متأكد من حذف {count} عنصر(ات)؟',
  'message.confirm.unsavedChanges': 'لديك تغييرات غير محفوظة. هل أنت متأكد من المغادرة؟',

  'message.loading': 'جاري التحميل...',
  'message.noData': 'لا توجد بيانات',
  'message.noResults': 'لم يتم العثور على نتائج',
  'message.searchTip': 'حاول تعديل معايير البحث',
  'message.emptyList': 'لا توجد عناصر بعد',
  'message.getStarted': 'ابدأ بإنشاء العنصر الأول',

  // ============================================
  // Table & List
  // ============================================
  'table.showing': 'عرض',
  'table.of': 'من',
  'table.results': 'نتائج',
  'table.page': 'صفحة',
  'table.perPage': 'لكل صفحة',
  'table.selected': 'محدد',
  'table.noData': 'لا توجد بيانات للعرض',
  'table.searchResults': 'نتائج البحث',

  // ============================================
  // Dashboard Stats
  // ============================================
  'stats.today': 'اليوم',
  'stats.upcoming': 'القادمة',
  'stats.total': 'الإجمالي',
  'stats.new': 'جديد',
  'stats.thisMonth': 'هذا الشهر',
  'stats.monthlyRevenue': 'الإيرادات الشهرية',
  'stats.totalPatients': 'إجمالي المرضى',
  'stats.activeStaff': 'الموظفون النشطون',
  'stats.systemHealth': 'حالة النظام',
  'stats.apiStatus': 'حالة API',
  'stats.database': 'قاعدة البيانات',
  'stats.modules': 'الوحدات',
  'stats.alerts': 'التنبيهات',
  'stats.allOperational': 'جميع الأنظمة تعمل',
  'stats.connected': 'متصل',
  'stats.needsAttention': 'عناصر تحتاج اهتمام',

  // ============================================
  // Clinic Specific
  // ============================================
  'clinic.doctor': 'طبيب',
  'clinic.nurse': 'ممرض/ة',
  'clinic.receptionist': 'موظف استقبال',
  'clinic.diagnosis': 'التشخيص',
  'clinic.treatment': 'العلاج',
  'clinic.prescription': 'الوصفة الطبية',
  'clinic.vitals': 'العلامات الحيوية',
  'clinic.bloodPressure': 'ضغط الدم',
  'clinic.temperature': 'الحرارة',
  'clinic.weight': 'الوزن',
  'clinic.height': 'الطول',
  'clinic.allergies': 'الحساسية',
  'clinic.medicalHistory': 'التاريخ الطبي',
  'clinic.chiefComplaint': 'الشكوى الرئيسية',
  'clinic.followUp': 'المتابعة',

  // ============================================
  // Keyboard Shortcuts
  // ============================================
  'shortcuts.title': 'اختصارات لوحة المفاتيح',
  'shortcuts.new': 'جديد',
  'shortcuts.search': 'بحث',
  'shortcuts.refresh': 'تحديث',
  'shortcuts.delete': 'حذف المحدد',
  'shortcuts.tip': 'نصيحة: اضغط',
  'shortcuts.toFocusSearch': 'للتركيز على البحث',

  // ============================================
  // Export Options
  // ============================================
  'export.asCSV': 'تصدير كـ CSV',
  'export.asExcel': 'تصدير كـ Excel',
  'export.print': 'طباعة',

  // ============================================
  // Footer
  // ============================================
  'footer.copyright': '© {year} زينون كلينك. جميع الحقوق محفوظة.',
};

export const frenchTranslations: TranslationDictionary = {
  // ============================================
  // Common Actions
  // ============================================
  'action.save': 'Enregistrer',
  'action.cancel': 'Annuler',
  'action.delete': 'Supprimer',
  'action.edit': 'Modifier',
  'action.create': 'Créer',
  'action.add': 'Ajouter',
  'action.remove': 'Retirer',
  'action.search': 'Rechercher',
  'action.filter': 'Filtrer',
  'action.export': 'Exporter',
  'action.import': 'Importer',
  'action.refresh': 'Actualiser',
  'action.print': 'Imprimer',
  'action.download': 'Télécharger',
  'action.upload': 'Téléverser',
  'action.submit': 'Soumettre',
  'action.confirm': 'Confirmer',
  'action.back': 'Retour',
  'action.next': 'Suivant',
  'action.previous': 'Précédent',
  'action.close': 'Fermer',
  'action.view': 'Voir',
  'action.viewDetails': 'Voir les détails',
  'action.selectAll': 'Tout sélectionner',
  'action.deselectAll': 'Tout désélectionner',
  'action.reset': 'Réinitialiser',
  'action.clear': 'Effacer',
  'action.apply': 'Appliquer',
  'action.login': 'Connexion',
  'action.logout': 'Déconnexion',
  'action.register': 'S\'inscrire',

  // ============================================
  // Navigation
  // ============================================
  'nav.dashboard': 'Tableau de bord',
  'nav.appointments': 'Rendez-vous',
  'nav.patients': 'Patients',
  'nav.laboratory': 'Laboratoire',
  'nav.pharmacy': 'Pharmacie',
  'nav.radiology': 'Radiologie',
  'nav.audiology': 'Audiologie',
  'nav.hr': 'RH',
  'nav.financial': 'Finances',
  'nav.inventory': 'Inventaire',
  'nav.marketing': 'Marketing',
  'nav.admin': 'Admin',
  'nav.settings': 'Paramètres',
  'nav.reports': 'Rapports',

  // ============================================
  // Entities
  // ============================================
  'entity.patient.singular': 'Patient',
  'entity.patient.plural': 'Patients',
  'entity.appointment.singular': 'Rendez-vous',
  'entity.appointment.plural': 'Rendez-vous',
  'entity.encounter.singular': 'Consultation',
  'entity.encounter.plural': 'Consultations',
  'entity.invoice.singular': 'Facture',
  'entity.invoice.plural': 'Factures',
  'entity.prescription.singular': 'Ordonnance',
  'entity.prescription.plural': 'Ordonnances',
  'entity.labTest.singular': 'Analyse',
  'entity.labTest.plural': 'Analyses',
  'entity.employee.singular': 'Employé',
  'entity.employee.plural': 'Employés',
  'entity.product.singular': 'Produit',
  'entity.product.plural': 'Produits',
  'entity.order.singular': 'Commande',
  'entity.order.plural': 'Commandes',

  // ============================================
  // Page Titles & Descriptions
  // ============================================
  'page.dashboard.title': 'Tableau de bord',
  'page.dashboard.welcome': 'Bon retour',
  'page.dashboard.overview': 'Voici l\'aperçu de votre clinique à travers tous les modules',

  'page.patients.title': 'Patients',
  'page.patients.description': 'Gérer les dossiers patients et informations',
  'page.patients.add': 'Nouveau patient',
  'page.patients.edit': 'Modifier le patient',
  'page.patients.recentlyViewed': 'Vus récemment',
  'page.patients.searchPlaceholder': 'Rechercher par nom, ID Emirates, téléphone ou email...',

  'page.appointments.title': 'Rendez-vous',
  'page.appointments.description': 'Planifier et gérer les rendez-vous',
  'page.appointments.add': 'Nouveau rendez-vous',
  'page.appointments.schedule': 'Planifier un rendez-vous',

  'page.laboratory.title': 'Laboratoire',
  'page.laboratory.description': 'Gérer les analyses et résultats',
  'page.laboratory.pendingOrders': 'Commandes en attente',
  'page.laboratory.urgentOrders': 'Commandes urgentes',

  'page.pharmacy.title': 'Pharmacie',
  'page.pharmacy.description': 'Gérer les ordonnances et médicaments',
  'page.pharmacy.pending': 'Ordonnances en attente',
  'page.pharmacy.dispensedToday': 'Dispensées aujourd\'hui',

  'page.radiology.title': 'Radiologie',
  'page.radiology.description': 'Gérer les commandes d\'imagerie et rapports',

  'page.audiology.title': 'Audiologie',
  'page.audiology.description': 'Gérer les tests auditifs et appareils',

  'page.hr.title': 'Ressources Humaines',
  'page.hr.description': 'Gérer les employés et la paie',
  'page.hr.totalEmployees': 'Total employés',
  'page.hr.activeEmployees': 'Employés actifs',

  'page.financial.title': 'Finances',
  'page.financial.description': 'Gérer les factures et transactions',
  'page.financial.revenue': 'Revenus',
  'page.financial.unpaidInvoices': 'Factures impayées',

  'page.inventory.title': 'Inventaire',
  'page.inventory.description': 'Gérer le stock et les fournitures',
  'page.inventory.totalItems': 'Total articles',
  'page.inventory.lowStock': 'Stock faible',

  'page.marketing.title': 'Marketing',
  'page.marketing.description': 'Gérer les campagnes et prospects',

  'page.admin.title': 'Administration',
  'page.admin.description': 'Administration système',

  'page.login.title': 'Connexion',
  'page.login.subtitle': 'Système de gestion de santé',
  'page.login.welcome': 'Bienvenue à XenonClinic',
  'page.login.noAccount': 'Pas de compte ?',
  'page.login.registerHere': 'Inscrivez-vous ici',
  'page.login.forgotPassword': 'Mot de passe oublié ?',
  'page.login.rememberMe': 'Se souvenir de moi',
  'page.login.signingIn': 'Connexion en cours...',

  // ============================================
  // Form Fields
  // ============================================
  'field.firstName': 'Prénom',
  'field.lastName': 'Nom',
  'field.fullName': 'Nom complet',
  'field.fullNameEn': 'Nom complet (Anglais)',
  'field.fullNameAr': 'Nom complet (Arabe)',
  'field.email': 'Email',
  'field.phone': 'Téléphone',
  'field.phoneNumber': 'Numéro de téléphone',
  'field.address': 'Adresse',
  'field.dateOfBirth': 'Date de naissance',
  'field.gender': 'Genre',
  'field.male': 'Homme',
  'field.female': 'Femme',
  'field.nationalId': 'ID National',
  'field.emiratesId': 'ID Emirates',
  'field.passportNumber': 'Numéro de passeport',
  'field.nationality': 'Nationalité',
  'field.date': 'Date',
  'field.time': 'Heure',
  'field.startDate': 'Date de début',
  'field.endDate': 'Date de fin',
  'field.status': 'Statut',
  'field.notes': 'Notes',
  'field.description': 'Description',
  'field.amount': 'Montant',
  'field.quantity': 'Quantité',
  'field.price': 'Prix',
  'field.total': 'Total',
  'field.discount': 'Remise',
  'field.tax': 'Taxe',
  'field.username': 'Nom d\'utilisateur',
  'field.password': 'Mot de passe',
  'field.age': 'Âge',
  'field.contact': 'Contact',
  'field.actions': 'Actions',

  // ============================================
  // Status Values
  // ============================================
  'status.active': 'Actif',
  'status.inactive': 'Inactif',
  'status.pending': 'En attente',
  'status.completed': 'Terminé',
  'status.cancelled': 'Annulé',
  'status.scheduled': 'Planifié',
  'status.inProgress': 'En cours',
  'status.draft': 'Brouillon',
  'status.approved': 'Approuvé',
  'status.rejected': 'Rejeté',
  'status.paid': 'Payé',
  'status.unpaid': 'Impayé',
  'status.overdue': 'En retard',
  'status.booked': 'Réservé',
  'status.confirmed': 'Confirmé',
  'status.checkedIn': 'Enregistré',
  'status.noShow': 'Absent',

  // ============================================
  // Messages
  // ============================================
  'message.success.saved': 'Enregistré avec succès',
  'message.success.created': 'Créé avec succès',
  'message.success.updated': 'Mis à jour avec succès',
  'message.success.deleted': 'Supprimé avec succès',
  'message.success.exported': 'Exporté avec succès',

  'message.error.generic': 'Une erreur s\'est produite',
  'message.error.notFound': 'Élément non trouvé',
  'message.error.unauthorized': 'Accès non autorisé',
  'message.error.validation': 'Veuillez vérifier les erreurs du formulaire',
  'message.error.network': 'Erreur réseau. Veuillez réessayer.',
  'message.error.loading': 'Erreur lors du chargement des données',

  'message.confirm.delete': 'Êtes-vous sûr de vouloir supprimer cet élément ?',
  'message.confirm.deleteMultiple': 'Êtes-vous sûr de vouloir supprimer {count} élément(s) ?',
  'message.confirm.unsavedChanges': 'Vous avez des modifications non enregistrées. Êtes-vous sûr de vouloir quitter ?',

  'message.loading': 'Chargement...',
  'message.noData': 'Aucune donnée disponible',
  'message.noResults': 'Aucun résultat trouvé',
  'message.searchTip': 'Essayez d\'ajuster vos critères de recherche',
  'message.emptyList': 'Aucun élément pour le moment',
  'message.getStarted': 'Commencez par créer votre premier élément',

  // ============================================
  // Table & List
  // ============================================
  'table.showing': 'Affichage',
  'table.of': 'sur',
  'table.results': 'résultats',
  'table.page': 'Page',
  'table.perPage': 'par page',
  'table.selected': 'sélectionné(s)',
  'table.noData': 'Aucune donnée à afficher',
  'table.searchResults': 'Résultats de recherche',

  // ============================================
  // Dashboard Stats
  // ============================================
  'stats.today': 'Aujourd\'hui',
  'stats.upcoming': 'À venir',
  'stats.total': 'Total',
  'stats.new': 'Nouveau',
  'stats.thisMonth': 'Ce mois',
  'stats.monthlyRevenue': 'Revenus mensuels',
  'stats.totalPatients': 'Total patients',
  'stats.activeStaff': 'Personnel actif',
  'stats.systemHealth': 'État du système',
  'stats.apiStatus': 'État API',
  'stats.database': 'Base de données',
  'stats.modules': 'Modules',
  'stats.alerts': 'Alertes',
  'stats.allOperational': 'Tous les systèmes opérationnels',
  'stats.connected': 'Connecté',
  'stats.needsAttention': 'éléments nécessitent attention',

  // ============================================
  // Clinic Specific
  // ============================================
  'clinic.doctor': 'Médecin',
  'clinic.nurse': 'Infirmier/ère',
  'clinic.receptionist': 'Réceptionniste',
  'clinic.diagnosis': 'Diagnostic',
  'clinic.treatment': 'Traitement',
  'clinic.prescription': 'Ordonnance',
  'clinic.vitals': 'Signes vitaux',
  'clinic.bloodPressure': 'Tension artérielle',
  'clinic.temperature': 'Température',
  'clinic.weight': 'Poids',
  'clinic.height': 'Taille',
  'clinic.allergies': 'Allergies',
  'clinic.medicalHistory': 'Antécédents médicaux',
  'clinic.chiefComplaint': 'Motif de consultation',
  'clinic.followUp': 'Suivi',

  // ============================================
  // Keyboard Shortcuts
  // ============================================
  'shortcuts.title': 'Raccourcis clavier',
  'shortcuts.new': 'Nouveau',
  'shortcuts.search': 'Rechercher',
  'shortcuts.refresh': 'Actualiser',
  'shortcuts.delete': 'Supprimer la sélection',
  'shortcuts.tip': 'Conseil : Appuyez sur',
  'shortcuts.toFocusSearch': 'pour focus sur la recherche',

  // ============================================
  // Export Options
  // ============================================
  'export.asCSV': 'Exporter en CSV',
  'export.asExcel': 'Exporter en Excel',
  'export.print': 'Imprimer',

  // ============================================
  // Footer
  // ============================================
  'footer.copyright': '© {year} XenonClinic. Tous droits réservés.',
};

/**
 * Get translations for a specific language code
 */
export function getTranslations(langCode: string): TranslationDictionary {
  switch (langCode) {
    case 'ar':
    case 'ar-AE':
      return arabicTranslations;
    case 'fr':
    case 'fr-FR':
      return frenchTranslations;
    default:
      return englishTranslations;
  }
}
