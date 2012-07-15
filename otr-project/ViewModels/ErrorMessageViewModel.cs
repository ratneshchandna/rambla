using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace otr_project.ViewModels
{
    public class ErrorMessageViewModel
    {
        public readonly string[] Headers = new string[] 
            {
                "Something went wrong on our side", 
                "Something went wrong while checking out with PayPal", 
                "Cannot Checkout", 
                "Cannot Checkout", 
                "Could not upload your image",
                "Could not upload your image",
                "An error occured when trying to upload your image",
                "An error occured when deleting the item"
            };

        public readonly string[] Descriptions = new string[] 
            {
                "", 
                "", 
                "Your cart is empty. Please add items to your cart that you wish to rent. ", 
                "You can't proceed to checkout because one of the items in your cart is owned by you. ", 
                "The image format you tried to upload is not supported. We support the following image formats: JPEG, PNG, GIF, BMP and ICO. Please go back and upload an image of a supported format. ",
                "The image you tried to upload exceeds the maximum requirements. Please go back and try uploading an image less than 500 KB. ",
                "",
                ""
            };

        public ErrorCode ErrorCode { get; set; }
        public string Header { get; set; }
        public string Description { get; set; }
    }
}

public enum ErrorCode
{
    UNKNOWN = 0, 
    PAYPAL_ERROR = 1, 
    EMPTY_CART = 2, 
    OWNER_ITEM_IN_CART = 3, 
    IMAGE_FORMAT_NOT_SUPPORTED = 4, 
    IMAGE_SIZE_TOO_BIG = 5, 
    IMAGE_UPLOAD_FAILED = 6, 
    ITEM_DELETE_FAILED = 7
}