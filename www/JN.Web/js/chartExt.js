//highstock
var highStockChart = function(divID,result,crrentData){
	var $reporting = $("#report");
	var firstTouch = true;
	var  open,high,low,close,y,zde,zdf,hsl,MA5,MA10,MA20,MA30,zs,relativeWidth; 

	var ohlcArray = [],volumeArray = [],MA5Array = [],MA10Array=[],MA20Array=[],MA30Array=[],zdfArray=[],zdeArray=[],hslArray=[],data=[],dailyData = [],data =[];	

	var showTips = 	function (minTime,maxTime,chart){

		chart.showLoading();

		var lowestPrice,highestPrice,array=[],highestArray=[],lowestArray=[],highestTime,lowestTime,flagsMaxData_1=[],flagsMaxData_2=[],flagsMinData_1,flagsMinData_2; 
		for(var i=0;i<ohlcArray.length-1;i++){
			if(ohlcArray[i][0]>=minTime && ohlcArray[i][0]<=maxTime){
				array.push([
				            ohlcArray[i][0],
				            ohlcArray[i][2], 
				            ohlcArray[i][3] 
				            ])
			}
		}
		if(!array.length>0){
			return;
		}
		highestArray = array.sort(function(x, y){  return y[1] - x[1];})[0];
		highestTime =highestArray[0];  
		highestPrice =highestArray[1].toFixed(4);  
		lowestArray = array.sort(function(x, y){  return x[2] - y[2];})[0]; 
		lowestTime =lowestArray[0];  
		lowestPrice =lowestArray[2].toFixed(4); 
		var formatDate1 = Highcharts.dateFormat('%Y-%m-%d',highestTime)
		var formatDate2 = Highcharts.dateFormat('%Y-%m-%d',lowestTime)
		flagsMaxData_1 = [
		               			{
		               			 x : highestTime,
		               			title : highestPrice
		               			}
		               		];
		
		flagsMaxData_2 = [
					               {
					                x : highestTime,
					                title : highestPrice
					               }
		               ];
		flagsMinData_1 = [
		                  {
		                	  x : lowestTime,
		                	  title : lowestPrice
		                  }
		                  ];
		
		flagsMinData_2 = [
		               {
		            	   x : lowestTime,
		            	   title : lowestPrice
		               }
		               ];
		var min =  parseFloat(flagsMinData_2[0].title) - parseFloat(flagsMinData_2[0].title)*0.05;
		var max =  parseFloat(flagsMaxData_2[0].title)+parseFloat(flagsMaxData_2[0].title)*0.05;
		var tickInterval = (( max-min)/5).toFixed(1)*1;
		var oneMonth = 1000*3600*24*30;
		var oneYear = 1000*3600*24*365;
		var tickIntervalTime,dataFormat='%Y-%m';
		if(maxTime-minTime>oneYear*2){
			tickIntervalTime = oneYear*2
			dataFormat = '%Y';
		}else if(maxTime-minTime>oneYear){
			tickIntervalTime = oneMonth*6
		}else if(maxTime-minTime>oneMonth*6){
			tickIntervalTime = oneMonth*3
		}else{
			tickIntervalTime = oneMonth
			dataFormat = '%m-%d'
		}
			

		 chart.yAxis[0].update({
	    	   	min : min,
	    	   	max : max,
	    	   	tickInterval: tickInterval
	       });
	
		 chart.xAxis[0].update({
			 min : minTime,
			 max : maxTime,
			 tickInterval: tickIntervalTime,
			 labels: {
				   	y:-78,
	             formatter: function(e) {
	             		 return Highcharts.dateFormat(dataFormat, this.value);
	             }
	         }
		 });

       chart.series[5].update({
    	   data : flagsMaxData_2,
            point:{
         	   events:{
         		}
         },
         events:{
             mouseOut:function(){
		             	 chart.series[5].update({
		   					data :flagsMaxData_2,
		   					width : 50
		             	 });
		             	 chart.series[6].update({
			   					data :flagsMinData_2,
			   					width : 50
			   			 });
             	}
         	}
		});
       
       chart.series[6].update({
    		data : flagsMinData_2,
    		   point:{
             	   events:{
             		 
                    }
             },
             events:{
                 mouseOut:function(){
		                	 chart.series[6].update({
				   					data :flagsMinData_2,
				   					width : 50
				   			 });
    		             	 chart.series[5].update({
    		   					data :flagsMaxData_2,
    		   					width : 50
    		             	 });
                 	}
             	}
		});
   	chart.hideLoading();
	}
	

	 var originalDrawPoints = Highcharts.seriesTypes.column.prototype.drawPoints;
	    Highcharts.seriesTypes.column.prototype.drawPoints = function () {
	        var merge  = Highcharts.merge,
	            series = this,
	            chart  = this.chart,
	            points = series.points,
	            i      = points.length;
	        
	        while (i--) {
	            var candlePoint = chart.series[0].points[i];
	            if(candlePoint.open != undefined && candlePoint.close !=  undefined){  
		            var color = (candlePoint.open < candlePoint.close) ? '#FF0000' : '#00FFFF';
		            var seriesPointAttr = merge(series.pointAttr);
		            seriesPointAttr[''].fill = color;
		            seriesPointAttr.hover.fill = Highcharts.Color(color).brighten(0.3).get();
		            seriesPointAttr.select.fill = color;
	            }else{
	            	var seriesPointAttr = merge(series.pointAttr);
	            }
	            
	            points[i].pointAttr = seriesPointAttr;
	        }
	
	        originalDrawPoints.call(this);
	    }

	

	dailyData = result.vl.split("~");	
	 
	for(i=0;i<dailyData.length-1;i++){
		data[i] = dailyData[i].split("^");
	}

	var length = data.length-1;
	var time = parseFloat(data[length][0]);
	var crrentTime = crrentData[0];

		
	for (i = 0; i < data.length; i++) {
		ohlcArray.push([
			parseInt(data[i][0]), // the date
			parseFloat(data[i][1]), // open
			parseFloat(data[i][3]), // high
			parseFloat(data[i][4]), // low
			parseFloat(data[i][2]) // close
		]);

		MA5Array.push([
	         parseInt(data[i][0]), // the date
	         parseFloat(data[i][11])
         ]);

		MA10Array.push([
	    	parseInt(data[i][0]),
	    	parseFloat(data[i][12]),
	     ]);
		MA20Array.push([
            parseInt(data[i][0]),
            parseFloat(data[i][13]),             
		                ])
		MA30Array.push([
	             	parseInt(data[i][0]),
	                parseFloat(data[i][14])
	        ]);
		  volumeArray.push([
				parseInt(data[i][0]), // the date
				parseInt(data[i][5]) 
			]);
	}


	return new Highcharts.StockChart( {
		chart:{
			renderTo : divID,
			margin: [1, 30,30, 10],
			plotBorderColor: '#f00',
			plotBorderWidth: 0.8,
			backgroundColor:'#000',
			events:{
				load:function(){
					var length = ohlcArray.length-1;
					showTips(ohlcArray[0][0],ohlcArray[length][0],this);	
				}
			}
		},
		loading: {
	    	labelStyle: {
                position: 'relative',
	            top: '10em',
	            zindex:1000
	    	}
	    },
		 credits:{
	            enabled:false
	        },
	    rangeSelector: {
			enabled:false,
	        inputDateFormat: '%Y-%m-%d' 
	    },
	    plotOptions: {
	
	    	candlestick: {
	    		color: '#00FFFF',
	    		upColor: '#FF0000',
	    		lineColor: '#00FFFF',	    		
	    		upLineColor: '#FF0000', 
	    		maker:{
	    			states:{
	    				hover:{
	    					enabled:false,
	    				}
	    			}
	    		}
	    	},
	    
            series: {
            	states: {
                    hover: {
                        enabled: false
                    }
                },
            line: {
                marker: {
                    enabled: false
                }
            }
            }
	    },
	 
	    tooltip: {
		   formatter: function() {
			   if(this.y == undefined){
				   return;
			   }
			   for(var i =0;i<data.length;i++){
				   if(this.x == data[i][0]){
					   zdf = parseFloat(data[i][7]).toFixed(2);
					   zde = parseFloat(data[i][8]).toFixed(4);
				//	   hsl = parseFloat(data[i][9]).toFixed(2);
					   zs = parseFloat(data[i][10]).toFixed(4);
				   }
			   }
			  
			   open = this.points[0].point.open.toFixed(4);
			   high = this.points[0].point.high.toFixed(4);
			   low = this.points[0].point.low.toFixed(4);
			   close = this.points[0].point.close.toFixed(4);
			   y = (this.points[1].point.y).toFixed(4);
			   MA5 =this.points[2].y.toFixed(4);
			   MA10 =this.points[3].y.toFixed(4);
			   MA30 =this.points[4].y.toFixed(4);
			   relativeWidth = this.points[0].point.shapeArgs.x;
			   var stockName = this.points[0].series.name;

		

		      var tip= '<b>'+ Highcharts.dateFormat('%Y-%m-%d  %A', this.x) +'</b><br/>';
		      tip +=stockName+"<br/>";
		      if(open>zs){
    			  tip += kline_openprice + ':<span style="color: #FF0000;">'+open+' </span><br/>';
    		  }else if(open == zs){
    			  tip += kline_openprice + ':<span style="color: #FFF;">'+open+' </span><br/>';
    		  }else{
    			  tip += kline_openprice + ':<span style="color: #33AA11;">'+open+' </span><br/>';
    		  } 
    		  if(high>zs){
    			  tip += kline_maxprice + ':<span style="color: #FF0000;">'+high+' </span><br/>';
    		  }else if(high==zs){
    			  tip += kline_maxprice + ':<span style="color: #FFF;">'+high+' </span><br/>';
    		  } else{
    			  tip += kline_maxprice + ':<span style="color: #33AA11;">'+high+' </span><br/>';
    		  } 
			   
    		  if(low>zs){
    			  tip += kline_minprice + ':<span style="color: #FF0000;">'+low+' </span><br/>';
    		  }else if(low == zs){
    			  tip += kline_minprice + ':<span style="color: #FFF;">'+low+' </span><br/>';
    		  }else{
    			  tip += kline_minprice + ':<span style="color: #33AA11;">'+low+' </span><br/>';
    		  }
    
    		  
    		  if(close>zs){
    			  tip += kline_closeprice + ':<span style="color: #FF0000;">'+close+' </span><br/>';
    		  }else if(close == zs){
    			  tip += kline_closeprice + ':<span style="color: #FFF;">'+close+' </span><br/>';
    		  }else{
    			  tip += kline_closeprice + ':<span style="color: #33AA11;">'+close+' </span><br/>';
    		  }
    		  
    		  if(zde>0){
    			  tip += kline_change + ':<span style="color: #FF0000;">'+zde+' </span><br/>';
    		  }else if(zde == 0){
    			  tip += kline_change + ':<span style="color: #FFF;">'+zde+' </span><br/>';
    		  }else{
    			  tip += kline_change + ':<span style="color: #33AA11;">'+zde+' </span><br/>';
    		  }
    		  
    		  if(zdf>0){
    			  tip += kline_range + ':<span style="color: #FF0000;">'+zdf+'% </span><br/>';
    		  }else if(zdf == 0){
    			  tip += kline_range + ':<span style="color: #FFF;">'+zdf+'% </span><br/>';
    		  }else{
    			  tip += kline_range + ':<span style="color: #33AA11;">'+zdf+'% </span><br/>';
    		  }
    		  
    		  tip += kline_count + ':' + y + " XCoin<br/>";
    	
    		  $reporting.html(
			  		'  <span>' + kline_date + ":"+	Highcharts.dateFormat('%Y-%m-%d',this.x) + '</span>'
    				+'  <span style="padding-left:5px;">' + kline_openprice + ":</span>" + open
    				+'  <span style="padding-left:5px;">' + kline_closeprice + ':</span>'+close
              		+'  <span style="padding-left:5px;">' + kline_maxprice + ':</span>'+ high
              		+'  <span style="padding-left:5px;">' + kline_minprice + ':</span>'+ low
              		+'	<b style="color:#fff;padding-left:5px">MA5:'+ MA5 + '</b>'
              		+'  <b style="color: #ffff00;padding-left:5px">MA10:'+ MA10 + '</b>'
              		+'  <b style="color:#BA55D3;padding-left:5px">MA30:'+ MA30 + '</b>'
              		);
    		  return tip;
		   },
		 //crosshairs:	[true, true]
		   crosshairs: {
   				dashStyle: 'dash'
		   },
   			borderColor:	'white',
	    	positioner: function () { 
	    		var halfWidth = this.chart.chartWidth/2;
	    		var width = this.chart.chartWidth-155;
	    		var height = this.chart.chartHeight/5-8;
	    		if(relativeWidth<halfWidth){
	    			return { x: width - 35, y:height };
	    		}else{
	    			return { x: 10, y: height };
	    		}
	    	},
	    	shadow: false
		},
	    title: {
	        enabled:false
	    },
	    exporting: { 
            enabled: false  
        }, 
		scrollbar: {
			enabled:false,
			barBackgroundColor: 'gray',
			barBorderRadius: 7,
			barBorderWidth: 0,
			buttonBackgroundColor: 'gray',
			buttonBorderWidth: 0,
			buttonArrowColor: 'yellow',
			buttonBorderRadius: 7,
			rifleColor: 'yellow',
			trackBackgroundColor: 'white',
			trackBorderWidth: 1,
			trackBorderColor: 'silver',
			trackBorderRadius: 7,
			//enabled: false,
			liveRedraw: false 
		},
		 navigator: {
			 enabled:false,
			 adaptToUpdatedData: false,
			 xAxis: {
				 labels: {
		             formatter: function(e) {
		             		 return Highcharts.dateFormat('%m-%d', this.value);
		             }
		         } 
			 },
			 handles: {
		    		backgroundColor: '#808080',
		    	},
		     margin:-10
		 },
	    xAxis: {
        	type: 'datetime',
        	 tickLength: 0,
			offset:100,
			lineColor:'#ff0000',
			lineWidth:1,
        	// minRange: 3600 * 1000*24*30, // one month
        	 events: {
        		 afterSetExtremes: function(e) {
 	    			var minTime = Highcharts.dateFormat("%Y-%m-%d", e.min);
 	    			var maxTime = Highcharts.dateFormat("%Y-%m-%d", e.max);
 	    			var chart = this.chart;
 	    			showTips(e.min,e.max,chart);
 	    		}
        	 }
    	},
	    yAxis: [{
	        title: {	
	           enable:false
	        },
	        height: '70%',
            gridLineColor: '#f00',
            gridLineWidth:0.5,
			gridLineDashStyle:'LongDash',
			minTickInterval:0.01,
          // gridLineDashStyle: 'longdash',
		    labels:{
	        	x:3
	        },
            opposite:true
	    },{
	        title: {
	           enabled:false
	        },
	        top: '75%',
	        height: '25%',	        
	        gridLineColor: '#f00',
            gridLineWidth:0.5,
			gridLineDashStyle:'LongDash'
	    }],
	    series: [
	    {
	    	type: 'candlestick',
	    	id:"candlestick",
	        name: result.cname,
	        data: ohlcArray,
	        dataGrouping: {
				enabled: false
			}
	    }
	    ,{
	        type: 'column',//2
	        name: 'Volume',
	        data: volumeArray,
	        yAxis: 1,
	        dataGrouping: {
				enabled: false
			}
	    } ,{
	        type: 'spline',
	        name: 'MA5',
	        color:'#fff',
	        data: MA5Array,
	        lineWidth:1.1,
	        dataGrouping: {
				enabled: false
			}
	    },{
	        type: 'spline',
	        name: 'MA10',
	        data: MA10Array,
	        color:'#ffff00',
	        threshold: null, 
	        lineWidth:1.1,
	        dataGrouping: {
				enabled: false
			}
	    },{
	        type: 'spline',
	        name: 'MA30',
	        data: MA30Array,
	        color:'#BA55D3',
	        threshold: null, 
	        lineWidth:1.1,
	        dataGrouping: {
				enabled: false
			}
	    },{
	    	 type : 'flags',
	           cursor:'pointer',
	           style:{
	        	   fontSize: '11px',
	               fontWeight: 'normal',
	               textAlign: 'center'
	           },
	           lineWidth:0.5,
	           onSeries : 'candlestick',
	           width : 50,
	           shape: 'squarepin'
	    },{
	    	 type : 'flags',
	         cursor:'pointer',
	         y: 33,
	         style:{
	        	   fontSize: '11px',
	               fontWeight: 'normal',
	               textAlign: 'center'
	           },
	           lineWidth:0.5,
	           onSeries : 'candlestick',
	           width : 50,
	           shape: 'squarepin'
	    }
	    ]
	});
}