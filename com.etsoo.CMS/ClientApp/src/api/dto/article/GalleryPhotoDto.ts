export type GalleryPhotoDto = {
  url: string;
  width: number;
  height: number;
  title?: string;
  description?: string;
  link?: string;
};

export type GalleryPhotoListItem = GalleryPhotoDto & {
  id: number;
};
