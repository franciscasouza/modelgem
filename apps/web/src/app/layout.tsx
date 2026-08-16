import type { Metadata } from "next";
import { Geist } from "next/font/google";
import { AuthProvider } from "@/components/auth/AuthProvider";
import "./globals.css";

const geistSans = Geist({
  variable: "--font-source-sans",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "ModelaFlow",
  description:
    "Studio web para modelagem assistida — bases paramétricas, canvas 2D e ficha técnica.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="pt-BR">
      <body className={geistSans.variable}>
        <AuthProvider>{children}</AuthProvider>
      </body>
    </html>
  );
}
