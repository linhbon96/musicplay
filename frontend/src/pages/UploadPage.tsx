import { Helmet } from "react-helmet-async";
import { Button, Form, Input, InputNumber, Upload, message, Typography } from "antd";
import { InboxOutlined } from "@ant-design/icons";
import { uploadTrack } from "../services/apiClient";

const { Title, Paragraph } = Typography;

export const UploadPage = () => {
  const [form] = Form.useForm();

  const handleFinish = async (values: Record<string, unknown>) => {
    try {
      const formData = new FormData();
      Object.entries(values).forEach(([key, value]) => {
        if (key === "file") {
          const fileList = value as { file: File }[];
          if (fileList?.length) {
            formData.append("file", fileList[0].file);
          }
        } else if (Array.isArray(value)) {
          value.forEach((item) => formData.append(`${key}[]`, String(item)));
        } else if (value !== undefined && value !== null) {
          formData.append(key, String(value));
        }
      });

      await uploadTrack(formData);
      message.success("Đã tải lên track thành công! Hãy chờ duyệt.");
      form.resetFields();
    } catch (error) {
      console.error(error);
      message.error("Không thể tải lên. Vui lòng thử lại.");
    }
  };

  return (
    <>
      <Helmet>
        <title>Tải nhạc lên MusicPlay</title>
      </Helmet>
      <section style={{ maxWidth: 720, margin: "0 auto" }}>
        <Title level={2}>Tải nhạc của bạn lên</Title>
        <Paragraph>
          Hệ thống hỗ trợ upload cho nghệ sĩ và admin với chuẩn streaming Range 206 giúp người nghe tua nhanh và không gián đoạn.
          Hãy bổ sung mô tả và thẻ để thuật toán SEO của MusicPlay đề xuất bạn đến đúng khán giả.
        </Paragraph>

        <Form form={form} layout="vertical" onFinish={handleFinish} requiredMark>
          <Form.Item name="title" label="Tên track" rules={[{ required: true, message: "Vui lòng nhập tên track" }]}> 
            <Input placeholder="Ví dụ: Đêm Trắng" />
          </Form.Item>
          <Form.Item name="artistName" label="Nghệ sĩ" rules={[{ required: true, message: "Vui lòng nhập nghệ sĩ" }]}> 
            <Input placeholder="Tên nghệ sĩ hoặc nhóm" />
          </Form.Item>
          <Form.Item name="description" label="Mô tả">
            <Input.TextArea rows={4} placeholder="Chia sẻ thông điệp của bài hát" />
          </Form.Item>
          <Form.Item name="genre" label="Thể loại">
            <Input placeholder="Ví dụ: Indie Pop" />
          </Form.Item>
          <Form.Item name="tags" label="Từ khóa (phân tách bởi dấu phẩy)">
            <Input placeholder="ví dụ: dreamy, chill, vietnam" />
          </Form.Item>
          <Form.Item name="duration" label="Thời lượng (giây)">
            <InputNumber min={0} style={{ width: "100%" }} />
          </Form.Item>
          <Form.Item
            name="file"
            label="Tệp âm thanh"
            valuePropName="fileList"
            getValueFromEvent={(event) => (Array.isArray(event) ? event : event?.fileList)}
            rules={[{ required: true, message: "Vui lòng chọn tệp âm thanh" }]}
          >
            <Upload.Dragger multiple={false} beforeUpload={() => false} accept="audio/*">
              <p className="ant-upload-drag-icon">
                <InboxOutlined />
              </p>
              <p className="ant-upload-text">Kéo & thả hoặc bấm để chọn file</p>
              <p className="ant-upload-hint">Hỗ trợ các định dạng phổ biến: MP3, WAV, FLAC</p>
            </Upload.Dragger>
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" size="large" block>
              Tải lên
            </Button>
          </Form.Item>
        </Form>
      </section>
    </>
  );
};
