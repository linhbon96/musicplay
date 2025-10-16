import { useMemo } from "react";
import { Helmet } from "react-helmet-async";
import {
  Card,
  Col,
  Row,
  Table,
  Tag,
  Typography,
  Statistic,
  Space,
  Button,
  message,
  Empty,
  Skeleton,
  Alert
} from "antd";
import { Column } from "@ant-design/plots";
import dayjs from "dayjs";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { approveTrack } from "../services/apiClient";
import { useAdminOverview } from "../hooks/useTracks";

const { Title, Paragraph, Text } = Typography;

export const AdminDashboardPage = () => {
  const queryClient = useQueryClient();
  const { data, isLoading } = useAdminOverview();
  const unauthorized = !isLoading && !data;

  const chartData = useMemo(
    () =>
      data?.uploads.map((item) => ({
        date: dayjs(item.date).format("DD/MM"),
        uploads: item.count
      })) ?? [],
    [data]
  );

  const approveMutation = useMutation({
    mutationFn: (trackId: string) => approveTrack(trackId),
    onSuccess: async () => {
      message.success("Đã phê duyệt track.");
      await queryClient.invalidateQueries({ queryKey: ["admin", "overview"] });
    },
    onError: () => {
      message.error("Không thể phê duyệt track, vui lòng thử lại.");
    }
  });

  return (
    <>
      <Helmet>
        <title>Bảng điều khiển Admin | MusicPlay</title>
      </Helmet>
      <Row gutter={[24, 24]}>
        <Col xs={24} lg={12}>
          <Card>
            <Title level={3}>Quản lý nội dung</Title>
            <Paragraph>
              Theo dõi hoạt động upload trong 7 ngày gần nhất, phê duyệt track mới và giữ cho thư viện nội dung luôn chất lượng.
            </Paragraph>
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card>
            <Title level={3}>Tài khoản thử nghiệm</Title>
            <Paragraph>
              Sử dụng các tài khoản có sẵn bên dưới để đăng nhập. Mật khẩu của cả hai tài khoản là MD5("123456") =
              <code>e10adc3949ba59abbe56e057f20f883e</code>.
            </Paragraph>
            <Table
              dataSource={[
                {
                  key: "admin",
                  username: "admin",
                  email: "admin@musicplay.local",
                  roles: ["Admin", "User"]
                },
                {
                  key: "user",
                  username: "user",
                  email: "user@musicplay.local",
                  roles: ["User"]
                }
              ]}
              columns={[
                { title: "Tài khoản", dataIndex: "username" },
                { title: "Email", dataIndex: "email" },
                {
                  title: "Quyền",
                  dataIndex: "roles",
                  render: (roles: string[]) => roles.map((role) => <Tag key={role}>{role}</Tag>)
                }
              ]}
              pagination={false}
              size="small"
              rowKey="key"
            />
          </Card>
        </Col>
      </Row>

      {unauthorized && (
        <Row gutter={[24, 24]} style={{ marginTop: 24 }}>
          <Col span={24}>
            <Alert
              type="warning"
              showIcon
              message="Bạn cần đăng nhập với quyền admin để xem thống kê thực tế."
              description="Hiện tại chỉ hiển thị thông tin mẫu. Sử dụng tài khoản admin để xem dữ liệu realtime và phê duyệt track."
            />
          </Col>
        </Row>
      )}

      <Row gutter={[24, 24]} style={{ marginTop: 24 }}>
        <Col xs={24} lg={6}>
          <Card>
            <Statistic title="Tổng track" value={data?.totals.tracks ?? 0} suffix=" bài" />
          </Card>
        </Col>
        <Col xs={24} lg={6}>
          <Card>
            <Statistic title="Chờ phê duyệt" value={data?.totals.pendingTracks ?? 0} suffix=" bài" valueStyle={{ color: "#f9a826" }} />
          </Card>
        </Col>
        <Col xs={24} lg={6}>
          <Card>
            <Statistic title="Người dùng" value={data?.totals.users ?? 0} suffix=" tài khoản" />
          </Card>
        </Col>
        <Col xs={24} lg={6}>
          <Card>
            <Statistic title="Lượt nghe" value={data?.totals.totalPlays ?? 0} suffix=" lần" />
          </Card>
        </Col>
      </Row>

      <Row gutter={[24, 24]} style={{ marginTop: 24 }}>
        <Col xs={24} lg={12}>
          <Card title="Biểu đồ upload 7 ngày">
            {isLoading ? (
              <Skeleton active paragraph={{ rows: 8 }} />
            ) : chartData.length > 0 ? (
              <Column
                data={chartData}
                xField="date"
                yField="uploads"
                color="#8a4fff"
                columnStyle={{ radius: [6, 6, 0, 0] }}
                tooltip={{ formatter: (datum) => ({ name: "Upload", value: `${datum.uploads} track` }) }}
              />
            ) : (
              <Empty description="Chưa có dữ liệu upload" />
            )}
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card title="Track chờ phê duyệt">
            {isLoading ? (
              <Skeleton active paragraph={{ rows: 6 }} />
            ) : data && data.pendingTracks.length > 0 ? (
              <Table
                dataSource={data.pendingTracks}
                rowKey={(record) => record.id}
                pagination={{ pageSize: 5 }}
                columns={[
                  {
                    title: "Tên track",
                    dataIndex: "title",
                    render: (value: string, record) => (
                      <Space direction="vertical" size={0}>
                        <strong>{value}</strong>
                        <Text type="secondary">{record.artistName}</Text>
                      </Space>
                    )
                  },
                  {
                    title: "Thể loại",
                    dataIndex: "genre",
                    render: (genre?: string) => genre ?? "--"
                  },
                  {
                    title: "Tags",
                    dataIndex: "tags",
                    render: (tags: string[]) =>
                      tags.length > 0 ? tags.map((tag) => <Tag key={tag}>{tag}</Tag>) : <Tag>Chưa cập nhật</Tag>
                  },
                  {
                    title: "Ngày upload",
                    dataIndex: "createdAt",
                    render: (date: string) => dayjs(date).format("DD/MM/YYYY")
                  },
                  {
                    title: "Thao tác",
                    dataIndex: "id",
                    render: (_: string, record) => (
                      <Button
                        type="primary"
                        onClick={() => approveMutation.mutate(record.id)}
                        loading={approveMutation.isPending}
                        disabled={record.isApproved}
                      >
                        Phê duyệt
                      </Button>
                    )
                  }
                ]}
              />
            ) : (
              <Empty description="Không có track chờ phê duyệt" />
            )}
          </Card>
        </Col>
      </Row>
    </>
  );
};
