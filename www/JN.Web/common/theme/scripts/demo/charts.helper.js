/* ==========================================================
 * AdminKIT v1.5
 * charts.helper.js
 * 
 * http://www.mosaicpro.biz
 * Copyright MosaicPro
 *
 * Built exclusively for sale @Envato Marketplaces
 * ========================================================== */ 

var charts = 
{
	// init charts on finances page
	initFinances: function()
	{
		// init simple chart
		this.chart_simple.init();
	},
	
	// init charts on Charts page
	initCharts: function()
	{
		// init simple chart
		this.chart_simple.init();
	},
	
	// init charts on dashboard
	initIndex: function()
	{
		// chart_ordered_bars
		this.chart_ordered_bars.init();

		// init traffic sources pie
		this.traffic_sources_pie.init();
	},

	// utility class
	utility:
	{
		chartColors: [ themerPrimaryColor, "#444", "#777", "#999", "#DDD", "#EEE" ],
		chartBackgroundColors: ["#fff", "#fff"],

		applyStyle: function(that)
		{
			that.options.colors = charts.utility.chartColors;
			that.options.grid.backgroundColor = { colors: charts.utility.chartBackgroundColors };
			that.options.grid.borderColor = charts.utility.chartColors[0];
			that.options.grid.color = charts.utility.chartColors[0];
		},
		
		// generate random number for charts
		randNum: function()
		{
			return (Math.floor( Math.random()* (1+40-20) ) ) + 20;
		}
	},
	
	// simple chart
	chart_simple:
	{
		// data
		data: 
		{
		    sin: [[20160618, 2], [20160619, 3], [20160620, 3]],
		    cos: [[20160618, 1], [20160619, 2], [20160620, 2]]
		},
		
		// will hold the chart object
		plot: null,

		// chart options
		options: 
		{
			grid: 
			{
				show: true,
			    aboveData: true,
			    color: "#3f3f3f",
			    labelMargin: 5,
			    axisMargin: 0, 
			    borderWidth: 0,
			    borderColor:null,
			    minBorderMargin: 5,
			    clickable: true, 
			    hoverable: true,
			    autoHighlight: true,
			    mouseActiveRadius: 20,
			    backgroundColor : { }
			},
	        series: {
	        	grow: {active: false},
	            lines: {
            		show: true,
            		fill: false,
            		lineWidth: 4,
            		steps: false
            	},
	            points: {
	            	show:true,
	            	radius: 5,
	            	symbol: "circle",
	            	fill: true,
	            	borderColor: "#fff"
	            }
	        },
	        legend: { position: "se" },
	        colors: [],
	        shadowSize:1,
	        tooltip: true, //activate tooltip
			tooltipOpts: {
				content: "%s : %y.3",
				shifts: {
					x: -30,
					y: -50
				},
				defaultTheme: false
			}
		},

		// initialize
		init: function()
		{
			// apply styling
			charts.utility.applyStyle(this);

			//if (this.plot == null)
			//{
			//	for (var i = 0; i < 14; i += 0.5) 
			//	{
			//        this.data.sin.push([i, Math.sin(i)]);
			//        this.data.cos.push([i, Math.cos(i)]);
			//    }
			//}
			this.plot = $.plot(
				$("#chart_simple"),
	           	[{
	    			label: "Sin", 
	    			data: this.data.sin,
	    			lines: {fillColor: "#DA4C4C"},
	    			points: {fillColor: "#fff"}
	    		}, 
	    		{	
	    			label: "Cos", 
	    			data: this.data.cos,
	    			lines: {fillColor: "#444"},
	    			points: {fillColor: "#fff"}
	    		}], this.options);
		}
	}
};