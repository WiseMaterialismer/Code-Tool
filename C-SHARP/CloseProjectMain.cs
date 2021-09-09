      //复制在cs文件,关闭相应的form窗口时,整个项目都会被关闭
	  protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_CLOSE = 0xF060;
            if (m.Msg == WM_SYSCOMMAND && (int)m.WParam == SC_CLOSE) Application.Exit();
            base.WndProc(ref m);
        }