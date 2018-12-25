%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe G:\公司项目\wl16071701\www\JN.Web\TimingPlan\JN.WindowsService.exe
Net Start wl16071701
sc config wl16071701 start= auto
pause