

//坏数字
function badFloat(num, size){
  if(isNaN(num)) return true;
  num += '';
  if(-1 == num.indexOf('.')) return false;
  var f_arr = num.split('.');
  if(f_arr[1].length > size){
    return true;
  }
  return false;
}

//格式化小数
function formatfloat(f, size){
  f += '';
  if(-1 == f.indexOf('.')) return (f = parseInt(f))? f: 0;

  var f_arr = f.split('.');
  
  if(f_arr[1].length > size){
    return parseFloat(parseFloat(f).toFixed(size));
  }
  return (f = parseFloat(f_arr[0] + '.' + f_arr[1])) ? f: 0;
}
//Dom
function Dom(o){
  return document.getElementById(o) ? document.getElementById(o) : false;
}

//总数据
function getData(){
  if(!ajaxRequest){
    return;
  }
  $.post(config.url+'/api/data?t=' + Math.random(),
    param,
    function(r){
      responseHandler.run(r, function(handler) {
        if(r.user != undefined){
          updateDataUser(r.user, r.rate);
        }
        if(r.statistic != undefined){
          updateDataStatistic(r.statistic, r.rate);
        }
        if(r.sum != undefined){
          updateDataSum(r.sum, param.sumLimit);
        }
        if(r.transaction != undefined){
          updateDataTransaction(r.transaction, param.transactionColumn);
        }
      });
    },
    'json'
  );
  gd = setTimeout("getData()", 30000);
}

//价格
function updateDataStatistic(data, rate){
  var n = target_currency+'_'+base_currency;
  Dom('time').innerHTML = data.time;
  var price = parseFloat(rate[n]).toFixed(2).split('.');
  Dom(n+'_rate').innerHTML = currency[base_currency].symbol + price[0];
  if(price[1]){
    Dom(n+'_rate').innerHTML += '<b>.'+price[1]+'</b>';
  }
  if(Dom(n+'_rate_market')){
    Dom(n+'_rate_market').innerHTML = currency[base_currency].symbol + price[0];
    if(price[1]){
      Dom(n+'_rate_market').innerHTML += '<b>.'+price[1]+'</b>';
    }
  }

  data.buy_price = parseFloat(data.buy_price);
  data.sell_price = parseFloat(data.sell_price);
  data.high_price = parseFloat(data.high_price);
  data.low_price = parseFloat(data.low_price);

  Dom('buy_price').innerHTML = data.buy_price;
  Dom('sell_price').innerHTML = data.sell_price;
  Dom('high_price').innerHTML = data.high_price;
  Dom('low_price').innerHTML = data.low_price;
  Dom('volume').innerHTML = data.volume;
}
//委托
function updateDataSum(data, limit){
  var with_scale = 1;
  for(var type in data){
    var d = data[type];
    var html = '';
    var idhtml = '';

    if(limit == 5 && type == 'sell'){
      var dmax = d.length > limit? limit: d.length;
    }
    var i = 1;
    for(var m=0;m<d.length;m++){
      var num = parseFloat(d[m][1]), width = num * with_scale > 100? 100: num * with_scale;
      if(type == 'buy'){
        idhtml = '<td class="buy">'+config.getString('buy')+'(' + (i) + ')</td>';
      }else{
        idhtml = '<td class="sell">'+config.getString('sell')+'(' + (limit == 5 ? dmax-- : i) + ')</td>';
      }
      html += '<tr>' + idhtml + '<td>'+ currency[base_currency].symbol + parseFloat(d[m][0]) + '</td><td>' + currency[target_currency].symbol + num + '</td><td><span style="width:' + width + 'px" class="' + (type == 'buy'? 'sell': 'buy') + 'Span"></span></td></tr>';
      i++;
    }
    $('#' + type + 'list').html(html);
  }
}
//成交
function updateDataTransaction(data, column){
  var html = '';
  for(var i in data){
    var s = data[i][1] == 1 ? 'buy' : 'sell';
    if(column == 'full'){
      html += '<tr><td>' + data[i][0] + '</td><td class="' + s + '">' + (data[i][1] == 1? config.getString('buyIn'): config.getString('sellOut')) +
        '</td><td class="' + s + '">'+currency[base_currency].symbol + formatfloat(data[i][2], 5) + '</td><td>' + currency[target_currency].symbol + formatfloat(data[i][3], 5) + '</td>' +
        '<td>'+currency[base_currency].symbol + formatfloat(data[i][2] * data[i][3], 5) + '</td></tr>';
    }else{
      html += '<tr><td>' + data[i][0] + '</td><td class="' + s + '">'+currency[base_currency].symbol + formatfloat(data[i][2], 5) + '</td><td>' + currency[target_currency].symbol + formatfloat(data[i][3], 5) + '</td>' +
        '</tr>';
    }
  }
  $('#transaction').html(html);
}
//最大可买
function maxBuy(price, balance){
  price = formatfloat(price, 2);
  if(balance > 0 && price > 0){
    $('#max').html(formatfloat(balance / price, 2));
  }
}
//总价
function buysumprice() {
    var total = formatfloat(formatfloat($('#buyprice').val()) * formatfloat($('#buynumber').val()), 5);
    $('#buymoney').val(total);
  return total;
}
function sellsumprice() {
    var total = formatfloat(formatfloat($('#sellprice').val()) * formatfloat($('#sellnumber').val()), 5);
    $('#sellmoney').val(total);
    return total;
}
//Fee
function getFee(source, target, balance){
  var total = sumprice();
  if(total > 0){
    $.post(config.url+'/api/getFee?t=' + Math.random(),
      {source : source, target : target, total : total},
      function(r){
        responseHandler.run(r, function(handler) {
          if(r.fee != undefined){
            $('#sumprice').html(total + ' + ' + formatfloat(r.fee, 5));
            var totalWithFee = parseFloat(total) + parseFloat(r.fee);
            //alert(balance);
            //alert(totalWithFee);
            if(totalWithFee <= parseFloat(balance)){
              $('#sumprice').removeClass().addClass('green');
            }else{
              $('#sumprice').removeClass().addClass('red');
            }
          }
        });
      },
      'json'
    );
  }
}
//验证价格
function vNum(o, len) {
  if(badFloat(o.value, len))
  {
		o.value=formatfloat(o.value, len);
  }
}

//function setSumLimit(v){
//  setParam({fetchSum : 1, sumLimit : v});
//  window.clearTimeout(gd);
//  getData();
//}

//function setTransactionLimit(v){
//  setParam({fetchTransaction : 1, transactionLimit : v});
//  window.clearTimeout(gd);
//  getData();
//}

function getPrice(){
  $.post(config.url+'/api/price?t=' + Math.random(),
    {currency : 'TTC'},
    function(r){
      responseHandler.run(r, function(handler) {
        var s = document.title.split('-');
        if(s[1] != undefined){
          document.title = r + '-' + s[1];
        }else{
          document.title = r + '-' + s[0];
        }
        $('#hd_price').text(r);
        $('#price').html(r);
      });
    }
  );
  setTimeout("getPrice()", 15000);
}



$(document).ready(function() {

  //getData();
  //if(typeof(isRealMobile) != 'undefined'){
  //  $('.sidebar .box').each(function(){
  //    $(this).children().slice(1).hide();
  //  });
  //  $('.sidebar .box .PlateTitle').each(function(){
  //    $(this).append('<div class="mr10 pull-right arrow"><i class="icon-chevron-down"></i></div>');
  //  });

  //  $('.sidebar .box .TitleBox').click(function(){
  //    $(this).parent().children().toggle();
  //    $(this).find('i').toggleClass('icon-chevron-down icon-chevron-up');
  //    $(this).show();
  //  });
  //}

  //getPrice();


});