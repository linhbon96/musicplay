import { ConfigProvider, theme } from "antd";
import { Route, Routes } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AppLayout } from "./layouts/AppLayout";
import { HomePage } from "./pages/HomePage";
import { DiscoverPage } from "./pages/DiscoverPage";
import { UploadPage } from "./pages/UploadPage";
import { AdminDashboardPage } from "./pages/AdminDashboardPage";
import { themeConfig } from "./theme";

const queryClient = new QueryClient();

export const App = () => {
  return (
    <QueryClientProvider client={queryClient}>
      <ConfigProvider
        theme={{
          ...themeConfig,
          algorithm: [theme.darkAlgorithm]
        }}
      >
        <Routes>
          <Route element={<AppLayout />}>
            <Route path="/" element={<HomePage />} />
            <Route path="/discover" element={<DiscoverPage />} />
            <Route path="/upload" element={<UploadPage />} />
            <Route path="/admin" element={<AdminDashboardPage />} />
          </Route>
        </Routes>
      </ConfigProvider>
    </QueryClientProvider>
  );
};
