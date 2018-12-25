﻿/* validator 基础设定 */ //jQuery.extend(jQuery.validator.defaults, { //    errorLabelContainer: $("#errorContent ul"), //    wrapper: "li", //    ignore: ".ignore", //    validClass: "valid", //    errorClass: "error", //    errorElement: "label" //});   //用主函数追加方式  /*整数 正整数 和 负整数*/ jQuery.validator.addMethod("integer", function (value, element) {     return this.optional(element) || /^\-?\d+$/.test(value); }, "必须是一个整数，正整数或负整数！");  /*手机号码*/ jQuery.validator.addMethod("mobile", function (value, element) {     return this.optional(element) || value.length == 11 && /^1[34578]\d{9}$/.test(value); }, "请输入正确的手机号！");  /*QQ*/ jQuery.validator.addMethod("qq", function (value, element) {     return this.optional(element) || /^\d{4,}$/.test(value); }, "请输入正确的 QQ 号！");  /*身份证*/ jQuery.validator.addMethod("idCard", function (value, element) {     return this.optional(element) || /^\d{15}|(\d{17}[\d|x|X])$/.test(value); }, "请输入正确的身份证号！");  jQuery.validator.addMethod("gh", function (value, element) {
    return this.optional(element) || /^gh\_[a-z|\d]{12}$/.test(value);
}, "请填写正确的公众号原始ID！");  