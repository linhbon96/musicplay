import { Helmet } from "react-helmet-async";
import { Card, Col, Row, Table, Tag, Typography } from "antd";

const { Title, Paragraph } = Typography;

const columns = [
  {
    title: "Tài khoản",
    dataIndex: "username"
  },
  {
    title: "Email",
    dataIndex: "email"
  },
  {
    title: "Quyền",
    dataIndex: "roles",
    render: (roles: string[]) => roles.map((role) => <Tag key={role}>{role}</Tag>)
  }
];

const sampleAccounts = [
  {
    key: "admin",
    username: "admin",
    email: "admin@musicplay.local",
    roles: ["Admin", "User"],
    passwordHash: "e10adc3949ba59abbe56e057f20f883e"
  },
  {
    key: "user",
    username: "user",
    email: "user@musicplay.local",
    roles: ["User"],
    passwordHash: "e10adc3949ba59abbe56e057f20f883e"
  }
];

export const AdminDashboardPage = () => (
  <>
    <Helmet>
      <title>Bảng điều khiển Admin | MusicPlay</title>
    </Helmet>
    <Row gutter={[24, 24]}>
      <Col xs={24} md={12}>
        <Card>
          <Title level={3}>Quản lý nội dung</Title>
          <Paragraph>
            Admin có thể duyệt track mới, chỉnh sửa playlist đề xuất và kích hoạt chiến dịch SEO cho nghệ sĩ nổi bật.
          </Paragraph>
        </Card>
      </Col>
      <Col xs={24} md={12}>
        <Card>
          <Title level={3}>Tài khoản thử nghiệm</Title>
          <Paragraph>
            Sử dụng các tài khoản có sẵn bên dưới để đăng nhập. Mật khẩu của cả hai tài khoản là MD5("123456") = <code>e10adc3949ba59abbe56e057f20f883e</code>.
          </Paragraph>
          <Table
            dataSource={sampleAccounts}
            columns={columns}
            pagination={false}
            size="small"
            rowKey="key"
          />
        </Card>
      </Col>
    </Row>
  </>
);
