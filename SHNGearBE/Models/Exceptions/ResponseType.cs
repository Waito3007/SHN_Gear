using System.ComponentModel;

namespace SHNGearBE.Models.Exceptions;

public enum ResponseType
{
    #region Success (1xx)

    [Description("Thành công")]
    Success = 100,
    [Description("Tạo mới thành công")]
    Created = 101,
    [Description("Cập nhật thành công")]
    Updated = 102,
    [Description("Xóa thành công")]
    Deleted = 103,
    [Description("Không có nội dung")]
    NoContent = 104,

    #endregion

    #region Validation Errors (1xxx)

    [Description("Tên không được trống")]
    NameCannotBeEmpty = 1001,
    [Description("ID không được trống")]
    IDCannotBeEmpty = 1002,
    [Description("Dữ liệu không hợp lệ")]
    InvalidData = 1003,
    [Description("Giá trị không hợp lệ")]
    InvalidValue = 1004,
    [Description("Ko được để trống ảnh")]
    ImageCannotBeEmpty = 1005,
    [Description("Slug không được trống")]
    SlugCannotBeEmpty = 1006,
    [Description("CategoryId không được trống")]
    CategoryIdCannotBeEmpty = 1007,
    [Description("BrandId không được trống")]
    BrandIdCannotBeEmpty = 1008,
    [Description("SKU không được trống")]
    SkuCannotBeEmpty = 1009,
    [Description("Giá phải lớn hơn 0")]
    PriceMustBePositive = 1010,
    [Description("Giá sale không được lớn hơn giá gốc")]
    SalePriceCannotExceedOriginalPrice = 1011,
    [Description("Số lượng tồn kho không được âm")]
    StockCannotBeNegative = 1012,

    #endregion

    #region Not Found Errors (2xxx)

    [Description("Không tìm thấy dữ liệu")]
    NotFound = 2001,
    [Description("Entity không tồn tại")]
    EntityNotFound = 2002,
    [Description("Sản phẩm không tồn tại")]
    ProductNotFound = 2003,
    [Description("Thương hiệu không tồn tại")]
    BrandNotFound = 2004,

    #endregion

    #region Client Errors (4xxx)

    [Description("Yêu cầu không hợp lệ")]
    BadRequest = 4000,
    [Description("Không có quyền truy cập")]
    Unauthorized = 4001,
    [Description("Bị cấm truy cập")]
    Forbidden = 4003,
    [Description("Dữ liệu bị xung đột")]
    Conflict = 4009,
    [Description("Dữ liệu đã tồn tại")]
    AlreadyExists = 4010,
    [Description("Slug đã tồn tại")]
    SlugAlreadyExists = 4011,

    #endregion

    #region Server Errors (5xxx)

    [Description("Lỗi hệ thống")]
    InternalServerError = 5000,
    [Description("Dịch vụ không khả dụng")]
    ServiceUnavailable = 5003,
    [Description("Lỗi kết nối database")]
    DatabaseError = 5004,

    #endregion
}