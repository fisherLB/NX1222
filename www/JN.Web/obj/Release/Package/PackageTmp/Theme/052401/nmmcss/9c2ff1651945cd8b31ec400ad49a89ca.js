function format_monetary_value(val){
	var currency_code = 'RMB';
	var currency_decimal = '2';
	var currency_decimal_point = '.';
	var currency_thousands_sep = ',';
	
	var number = number_format(val, currency_decimal, currency_decimal_point, currency_thousands_sep);
	money_value = '{currency_code}{amount}';
	money_value = money_value.replace(/{currency_code}/, currency_code);
	money_value = money_value.replace(/{amount}/, number);
	return money_value;
}
