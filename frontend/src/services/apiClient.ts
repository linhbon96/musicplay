import axios from "axios";

export const apiClient = axios.create({
  baseURL: "/api"
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem("musicplay.token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export interface TrackDto {
  id: string;
  title: string;
  artistName: string;
  description?: string;
  genre?: string;
  tags: string[];
  duration: number;
  streamUrl: string;
  coverUrl?: string;
  featured: boolean;
  playCount: number;
  createdAt: string;
}

export interface PaginatedTracksResponse {
  total: number;
  page: number;
  pageSize: number;
  items: TrackDto[];
}

export const fetchFeaturedTracks = async () => {
  const { data } = await apiClient.get<TrackDto[]>("/tracks/featured");
  return data;
};

export const fetchTracks = async (page = 1, pageSize = 12) => {
  const { data } = await apiClient.get<PaginatedTracksResponse>("/tracks", {
    params: { page, pageSize }
  });
  return data;
};

export interface PlaylistDto {
  id: string;
  title: string;
  description?: string;
  tracks: TrackDto[];
  coverUrl?: string;
  isPublic: boolean;
}

export const fetchFeaturedPlaylists = async () => {
  const { data } = await apiClient.get<PlaylistDto[]>("/playlists/featured");
  return data;
};

export interface AuthResponse {
  token: string;
  user: {
    id: string;
    username: string;
    email: string;
    roles: string[];
  };
}

export const login = async (identifier: string, password: string) => {
  const { data } = await apiClient.post<AuthResponse>("/auth/login", { identifier, password });
  localStorage.setItem("musicplay.token", data.token);
  return data;
};

export const register = async (payload: {
  username: string;
  email: string;
  password: string;
  displayName?: string;
  bio?: string;
}) => {
  const { data } = await apiClient.post<AuthResponse>("/auth/register", payload);
  localStorage.setItem("musicplay.token", data.token);
  return data;
};

export const uploadTrack = async (form: FormData) => {
  const { data } = await apiClient.post("/tracks/upload", form, {
    headers: { "Content-Type": "multipart/form-data" }
  });
  return data;
};
